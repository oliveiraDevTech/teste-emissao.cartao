using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Core.Application.Interfaces;
using Core.Application.Interfaces.Repositories;
using Driven.SqlLite.Data;
using Driven.SqlLite.Repositories;

namespace Driven.SqlLite;

/// <summary>
/// Extensão para configurar a injeção de dependências do Driven.SqlLite
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adiciona os serviços de banco de dados SQLite ao container de DI
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <param name="connectionString">String de conexão do SQLite</param>
    /// <returns>IServiceCollection para encadeamento</returns>
    public static IServiceCollection AddSqlLiteDatabase(this IServiceCollection services, string connectionString)
    {
        // Registrar o DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite(connectionString)
        );

        // Registrar repositórios de cartões
        services.AddScoped<ICardRepository, CardRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();

        return services;
    }
}
