using FCG.Contracts.Events;
using MassTransit;

namespace FCG.PaymentsAPI.Consumers;

public class OrderPlacedConsumer : IConsumer<OrderPlacedEvent>
{
    private readonly ILogger<OrderPlacedConsumer> _logger;
    private readonly IPublishEndpoint _publishEndpoint;

    // Taxa de aprovação configurável (padrão 90%)
    private static readonly Random _rng = new();

    public OrderPlacedConsumer(ILogger<OrderPlacedConsumer> logger, IPublishEndpoint publishEndpoint)
    {
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }

    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var evt = context.Message;

        _logger.LogInformation(
            "[PAGAMENTO] 📥 Pedido recebido | OrderId: {OrderId} | UserId: {UserId} | Jogo: {GameName} | Valor: R$ {Price:F2}",
            evt.OrderId, evt.UserId, evt.GameName, evt.Price);

        // Simula latência de processamento (gateway externo)
        await Task.Delay(TimeSpan.FromMilliseconds(200), context.CancellationToken);

        // Simula aprovação/rejeição (90% aprovação)
        var aprovado = _rng.NextDouble() < 0.90;

        PaymentProcessedEvent resultado;

        if (aprovado)
        {
            resultado = new PaymentProcessedEvent(
                OrderId: evt.OrderId,
                UserId: evt.UserId,
                GameId: evt.GameId,
                GameName: evt.GameName,
                Price: evt.Price,
                Status: "Approved",
                TransactionId: Guid.NewGuid().ToString("N").ToUpper(),
                MotivoRejeicao: null,
                ProcessedAt: DateTime.UtcNow);

            _logger.LogInformation(
                "[PAGAMENTO] ✅ Pagamento APROVADO | OrderId: {OrderId} | TransactionId: {TransactionId} | Valor: R$ {Price:F2}",
                evt.OrderId, resultado.TransactionId, evt.Price);
        }
        else
        {
            var motivos = new[]
            {
                "Saldo insuficiente",
                "Cartão recusado pela operadora",
                "Limite diário atingido",
                "Transação bloqueada por segurança"
            };
            var motivo = motivos[_rng.Next(motivos.Length)];

            resultado = new PaymentProcessedEvent(
                OrderId: evt.OrderId,
                UserId: evt.UserId,
                GameId: evt.GameId,
                GameName: evt.GameName,
                Price: evt.Price,
                Status: "Rejected",
                TransactionId: null,
                MotivoRejeicao: motivo,
                ProcessedAt: DateTime.UtcNow);

            _logger.LogWarning(
                "[PAGAMENTO] ❌ Pagamento RECUSADO | OrderId: {OrderId} | Motivo: {Motivo}",
                evt.OrderId, motivo);
        }

        await _publishEndpoint.Publish(resultado, context.CancellationToken);

        _logger.LogInformation(
            "[PAGAMENTO] 📤 PaymentProcessedEvent publicado | OrderId: {OrderId} | Status: {Status}",
            evt.OrderId, resultado.Status);
    }
}
