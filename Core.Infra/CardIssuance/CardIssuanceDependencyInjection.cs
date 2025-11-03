using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Core.Application.Interfaces;
using Core.Application.Options;

namespace Core.Infra.CardIssuance;

/// <summary>
/// Extensão para configurar injeção de dependências de emissão de cartões
/// </summary>
public static class CardIssuanceDependencyInjection
{
    /// <summary>
    /// Adiciona serviços de emissão de cartões ao container DI
    /// </summary>
    public static IServiceCollection AddCardIssuanceServices(this IServiceCollection services)
    {
        // Registrar gerador de PAN
        services.AddSingleton<IPanGenerator, PanGenerator>();

        // Registrar TokenVault
        services.AddSingleton<ITokenVault>(sp =>
        {
            var encryptionKey = Environment.GetEnvironmentVariable("TOKEN_VAULT_KEY")
                ?? "default-dev-key-32-chars-min!!!";
            return new InMemoryTokenVault(encryptionKey);
        });

        // Registrar opções de issuance
        services.Configure<CardIssuanceOptions>(options =>
        {
            options.BinVisa = "516233";
            options.BinMastercard = "453912";
            options.AnosValidade = 3;
        });

        // Registrar opções do Outbox Dispatcher
        services.Configure<OutboxDispatcherOptions>(options =>
        {
            options.IntervalMilisegundos = 1000;
            options.LoteTamanho = 100;
            options.DelayRetryMilisegundos = 500;
            options.DelayMaximoMilisegundos = 30000;
            options.MaximoTentativas = 5;
            options.FatorExponencial = 2.0;
            options.DiasRetencao = 7;
        });

        // Registrar OutboxDispatcher como hosted service
        services.AddSingleton(sp =>
        {
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<OutboxDispatcher>>();
            var options = sp.GetRequiredService<IOptions<OutboxDispatcherOptions>>().Value;
            return new OutboxDispatcher(logger, sp, options);
        });

        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<OutboxDispatcher>());

        return services;
    }

    /// <summary>
    /// Configura as opções de issuance
    /// </summary>
    public static IServiceCollection ConfigureCardIssuanceOptions(
        this IServiceCollection services,
        Action<CardIssuanceOptions> configureOptions)
    {
        services.Configure(configureOptions);
        return services;
    }
}
