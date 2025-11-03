using Microsoft.Extensions.DependencyInjection;
using Driven.CreditMock.Interfaces;
using Driven.CreditMock.Services;

namespace Driven.CreditMock;

/// <summary>
/// Extensão para configurar a injeção de dependências do Driven.CreditMock
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Adiciona o serviço mock de crédito ao container de DI
    /// Útil para desenvolvimento, testes e demonstração
    /// </summary>
    /// <param name="services">Container de serviços</param>
    /// <returns>IServiceCollection para encadeamento</returns>
    public static IServiceCollection AddCreditMockService(this IServiceCollection services)
    {
        services.AddSingleton<ICreditMockService, CreditMockService>();
        return services;
    }
}
