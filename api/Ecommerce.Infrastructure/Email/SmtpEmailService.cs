namespace Ecommerce.Infrastructure.Email;

using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken ct = default)
    {
        var smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST") ?? _configuration["SMTP_HOST"];
        var smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT") ?? _configuration["SMTP_PORT"] ?? "587");
        var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? _configuration["SMTP_USERNAME"];
        var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? _configuration["SMTP_PASSWORD"];

        if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
        {
            _logger.LogWarning("SMTP settings not configured. Email will be logged instead of sent.");
            _logger.LogInformation("[Email] To={To} Subject={Subject}", message.To, message.Subject);
            return;
        }

        try
        {
            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(smtpUsername, smtpPassword)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUsername, "E-commerce App"),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(message.To);

            await smtpClient.SendMailAsync(mailMessage, ct);
            _logger.LogInformation("Email sent successfully to {To}", message.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", message.To);
            _logger.LogInformation("[Email] To={To} Subject={Subject} (Failed to send, logged instead)", message.To, message.Subject);
        }
    }

    public Task SendEmailAsync(EmailMessage message, CancellationToken ct = default)
    {
        return SendAsync(message, ct);
    }
}
