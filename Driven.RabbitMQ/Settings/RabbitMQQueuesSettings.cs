namespace Driven.RabbitMQ.Settings;

/// <summary>
/// Configurações de nomes de filas do RabbitMQ
/// </summary>
public class RabbitMQQueuesSettings
{
    /// <summary>
    /// Nome da fila para eventos de cliente cadastrado
    /// </summary>
    public string ClienteCadastrado { get; set; } = "cliente.cadastrado";

    /// <summary>
    /// Nome da fila para eventos de análise de crédito completa
    /// </summary>
    public string AnaliseCreditoComplete { get; set; } = "analise.credito.complete";

    /// <summary>
    /// Nome da fila para pedidos de emissão de cartão
    /// </summary>
    public string CartaoEmissaoPedido { get; set; } = "cartao.emissao.pedido";

    /// <summary>
    /// Nome da fila para eventos de cartão emitido
    /// </summary>
    public string CartaoEmitido { get; set; } = "cartao.emitido";

    /// <summary>
    /// Nome da fila para eventos de falha na emissão de cartão
    /// </summary>
    public string CartaoEmissaoFalha { get; set; } = "cartao.emissao.falha";
}
