
namespace Ecommerce.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Domain.ValueObjects;
using Ecommerce.Domain.Entities;
public interface IPaymentService
{
    Task<PaymentResult> PayAsync(Order order, CancellationToken ct = default);
}
