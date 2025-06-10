
namespace Ecommerce.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Ecommerce.Domain.Models;
public interface IEmailService
{    
    Task SendAsync(EmailMessage message, CancellationToken ct = default);
    Task SendEmailAsync(EmailMessage message, CancellationToken ct = default);
}