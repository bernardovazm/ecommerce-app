using Microsoft.EntityFrameworkCore;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Infrastructure.Data;

public class PaymentRequestRepository : IPaymentRequestRepository
{
    private readonly AppDbContext _context;

    public PaymentRequestRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.PaymentRequests
            .FirstOrDefaultAsync(pr => pr.Id == id, cancellationToken);
    }

    public async Task<PaymentRequest> CreateAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        _context.PaymentRequests.Add(paymentRequest);
        await _context.SaveChangesAsync(cancellationToken);
        return paymentRequest;
    }

    public async Task UpdateAsync(PaymentRequest paymentRequest, CancellationToken cancellationToken = default)
    {
        _context.PaymentRequests.Update(paymentRequest);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentRequest>> GetPendingRequestsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentRequests
            .Where(pr => pr.Status == PaymentRequestStatus.Pending)
            .OrderBy(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PaymentRequest>> GetFailedRequestsForRetryAsync(CancellationToken cancellationToken = default)
    {
        return await _context.PaymentRequests
            .Where(pr => pr.Status == PaymentRequestStatus.Failed && pr.RetryCount < 3)
            .Where(pr => pr.CreatedAt < DateTime.UtcNow.AddMinutes(-5)) // Wait 5 minutes before retry
            .OrderBy(pr => pr.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
