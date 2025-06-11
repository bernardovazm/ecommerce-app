namespace Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Entities;

public interface IMessagePublisher
{
    Task PublishAsync<T>(string queueName, T message, CancellationToken cancellationToken = default);
    Task PublishPaymentRequestAsync(Guid paymentRequestId, CancellationToken cancellationToken = default);
}

public interface IMessageConsumer
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
}

public interface IPaymentRequestRepository
{
    Task<PaymentRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaymentRequest> CreateAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PaymentRequest>> GetFailedRequestsForRetryAsync(CancellationToken cancellationToken = default);
}
