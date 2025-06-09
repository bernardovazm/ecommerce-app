namespace Ecommerce.Infrastructure.Payments;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.ValueObjects;

public class PagarmePaymentService : IPaymentService
{
    public Task<PaymentResult> PayAsync(Order order, CancellationToken ct = default)
    {
        return Task.FromResult(new PaymentResult(true, Guid.NewGuid().ToString(), null));
    }
}
