using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Core.Application;
using Core.Infra.CardIssuance;
using Driven.SqlLite;
using Driven.SqlLite.Data;
using Driven.RabbitMQ;
using Driven.RabbitMQ.Interfaces;
using Core.Infra;

var builder = WebApplication.CreateBuilder(args);

// ========== CONFIGURAÇÃO DE SERVIÇOS ==========

// Adicionar Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Card Issuance API",
        Version = "v1",
        Description = "API para emissão e ativação de cartões de crédito",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Desenvolvimento Backend"
        }
    });

    // Configurar autenticação JWT no Swagger
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

// Obter JWT Secret das variáveis de ambiente ou usar padrão para desenvolvimento
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "sua_chave_super_secreta_com_minimo_32_caracteres_para_producao";

// Configurar autenticação JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = "CardIssuanceApi",
        ValidateAudience = true,
        ValidAudience = "CardIssuanceApp",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Injeção de dependências - Camada de Aplicação
builder.Services.AddApplicationServices(jwtSecret, "CardIssuanceApi", "CardIssuanceApp", 60);

// Injeção de dependências - Camada de Dados
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=card_issuance.db;";
builder.Services.AddSqlLiteDatabase(connectionString);

// Injeção de dependências - Infraestrutura (Logging, Cache, Email)
builder.Services.AddInfrastructureServices(builder.Configuration);

// Injeção de dependências - Emissão de Cartões
builder.Services.AddCardIssuanceServices();

// Injeção de dependências - RabbitMQ (Mensageria)
builder.Services.AddRabbitMQServices(builder.Configuration);

// ========== CONSTRUIR APLICAÇÃO ==========

var app = builder.Build();

// ========== CONFIGURAR HTTP REQUEST PIPELINE ==========

// Sempre usar Swagger (Development e Production)
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Card Issuance API v1");
    options.RoutePrefix = "swagger";
    options.DocumentTitle = "Card Issuance API";
});

if (app.Environment.IsDevelopment())
{
    // Configurações adicionais apenas para desenvolvimento
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ========== APLICAR MIGRATIONS E CRIAR BANCO AUTOMATICAMENTE ==========

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    Console.WriteLine("🔄 Criando banco de dados...");
    dbContext.Database.EnsureCreated();
    Console.WriteLine("✅ Banco de dados criado com sucesso!");
}

// ========== INICIALIZAR RABBITMQ (COM FALLBACK) ==========

try
{
    // Inicializa RabbitMQ se disponível
    var messageBus = app.Services.GetRequiredService<IMessageBus>();
    if (messageBus?.TryConnect() ?? false)
    {
        Console.WriteLine("✅ RabbitMQ conectado com sucesso");

        // ========== INICIALIZAR CONSUMIDORES DE EVENTOS ==========

        var subscriber = app.Services.GetRequiredService<IMessageSubscriber>();
        var pedidoEmissaoHandler = app.Services.GetRequiredService<Core.Application.Handlers.PedidoEmissaoCartaoEventHandler>();
        var logger = app.Services.GetRequiredService<Microsoft.Extensions.Logging.ILogger<Program>>();
        
        // Obter configurações do RabbitMQ incluindo nomes das filas
        var rabbitMQSettings = app.Configuration.GetSection("RabbitMQ").Get<Driven.RabbitMQ.Settings.RabbitMQSettings>();
        var queueName = rabbitMQSettings?.Queues?.CartaoEmissaoPedido ?? "cartao.emissao.pedido";

        // Subscrever ao evento de pedido de emissão de cartão
        try
        {
            await subscriber.SubscribeAsync<Driven.RabbitMQ.Events.PedidoEmissaoCartaoIntegrationEvent>(
                queueName: queueName,
                handler: async (evento) =>
                {
                    logger.LogInformation("Evento PedidoEmissaoCartaoIntegrationEvent recebido para cliente {ClienteId}", evento.ClienteId);
                    await pedidoEmissaoHandler.HandleAsync(evento);
                });

            Console.WriteLine("✅ Consumer de PedidoEmissaoCartaoIntegrationEvent inicializado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao inicializar consumer de PedidoEmissaoCartaoIntegrationEvent");
        }
    }
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"⚠️  Aviso: RabbitMQ não foi inicializado. {ex.Message}");
    Console.WriteLine("A aplicação continuará funcionando sem mensageria.");
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️  Aviso: Erro ao conectar ao RabbitMQ: {ex.Message}");
    Console.WriteLine("A aplicação continuará funcionando sem mensageria.");
}

app.Run();
