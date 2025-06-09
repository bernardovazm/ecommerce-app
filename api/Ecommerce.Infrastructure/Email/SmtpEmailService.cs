namespace Ecommerce.Infrastructure.Email;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

public class SmtpEmailService : IEmailService
{    public Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        Console.WriteLine($"[Email] To={message.To} Subject={message.Subject}");
        return Task.CompletedTask;
    }
}
