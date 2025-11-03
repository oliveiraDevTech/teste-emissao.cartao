using Microsoft.EntityFrameworkCore;
using Core.Domain.Entities;

namespace Driven.SqlLite.Data;

/// <summary>
/// Contexto de banco de dados para a aplicação
/// Gerencia a persistência de dados usando Entity Framework Core e SQLite
/// </summary>
public class ApplicationDbContext : DbContext
{
    /// <summary>
    /// DbSet para a entidade Cliente
    /// </summary>
    public DbSet<Cliente> Clientes { get; set; }

    // TODO: Adicionar entidade Usuario quando implementada

    /// <summary>
    /// DbSet para a entidade Cartão
    /// </summary>
    public DbSet<Card> Cards { get; set; }

    /// <summary>
    /// DbSet para registros de idempotência de emissão de cartões
    /// </summary>
    public DbSet<CardIdempotencyKey> CardIdempotencyKeys { get; set; }

    /// <summary>
    /// DbSet para eventos do padrão Outbox
    /// </summary>
    public DbSet<OutboxEvent> OutboxEvents { get; set; }

    /// <summary>
    /// Construtor do contexto
    /// </summary>
    /// <param name="options">Opções de configuração do DbContext</param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Configura o modelo do banco de dados
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar a entidade Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            // Chave primária
            entity.HasKey(e => e.Id);

            // Propriedades
            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.Nome)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.Telefone)
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.Cpf)
                .IsRequired()
                .HasMaxLength(14);

            entity.Property(e => e.Endereco)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Cidade)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Estado)
                .IsRequired()
                .HasMaxLength(2);

            entity.Property(e => e.Cep)
                .IsRequired()
                .HasMaxLength(9);

            entity.Property(e => e.DataCriacao)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);

            entity.Property(e => e.DataAtualizacao)
                .IsRequired(false);

            entity.Property(e => e.Ativo)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Cliente_Email");

            entity.HasIndex(e => e.Cpf)
                .IsUnique()
                .HasDatabaseName("IX_Cliente_Cpf");

            entity.HasIndex(e => e.Nome)
                .HasDatabaseName("IX_Cliente_Nome");

            entity.HasIndex(e => e.Ativo)
                .HasDatabaseName("IX_Cliente_Ativo");

            // Nome da tabela
            entity.ToTable("Clientes");
        });

        // TODO: Configurar a entidade Usuario quando implementada

        // Configurar a entidade Card
        modelBuilder.Entity<Card>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .IsRequired();

            entity.Property(e => e.ClienteId).IsRequired();
            entity.Property(e => e.PropostaId).IsRequired();
            entity.Property(e => e.ContaId).IsRequired();
            entity.Property(e => e.CodigoProduto).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(20);
            entity.Property(e => e.TokenPan).IsRequired().HasMaxLength(100);
            entity.Property(e => e.TokenCvv).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Status).IsRequired().HasMaxLength(50).HasDefaultValue("REQUESTED");
            entity.Property(e => e.CanalAtivacao).HasMaxLength(50);
            entity.Property(e => e.CorrelacaoId).IsRequired().HasMaxLength(100);

            entity.HasIndex(e => e.ClienteId).HasDatabaseName("IX_Card_ClienteId");
            entity.HasIndex(e => e.PropostaId).HasDatabaseName("IX_Card_PropostaId");
            entity.HasIndex(e => e.ContaId).HasDatabaseName("IX_Card_ContaId");
            entity.HasIndex(e => e.Status).HasDatabaseName("IX_Card_Status");

            entity.ToTable("Cards");
        });

        // Configurar CardIdempotencyKey
        modelBuilder.Entity<CardIdempotencyKey>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ChaveIdempotencia)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.CartoesIds)
                .IsRequired()
                .HasMaxLength(1000); // JSON array dos IDs

            entity.HasIndex(e => e.ChaveIdempotencia)
                .IsUnique()
                .HasDatabaseName("IX_CardIdempotencyKey_Chave");

            entity.ToTable("CardIdempotencyKeys");
        });

        // Configurar OutboxEvent
        modelBuilder.Entity<OutboxEvent>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Topico)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Payload)
                .IsRequired();

            entity.Property(e => e.DataCriacao)
                .IsRequired()
                .HasDefaultValue(DateTime.UtcNow);

            entity.HasIndex(e => e.Topico).HasDatabaseName("IX_OutboxEvent_Topico");
            entity.HasIndex(e => e.DataEnvio).HasDatabaseName("IX_OutboxEvent_DataEnvio");

            entity.ToTable("OutboxEvents");
        });
    }
}
