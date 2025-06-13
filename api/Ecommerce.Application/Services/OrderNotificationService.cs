using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services;

public interface IOrderNotificationService
{
    Task SendOrderConfirmationEmailAsync(Order order, CancellationToken cancellationToken = default);
}

public class OrderNotificationService : IOrderNotificationService
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderNotificationService> _logger;

    public OrderNotificationService(IEmailService emailService, ILogger<OrderNotificationService> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task SendOrderConfirmationEmailAsync(Order order, CancellationToken cancellationToken = default)
    {
        try
        {
            var customerEmail = order.Customer?.Email ?? "customer@example.com";
            var customerName = $"{order.Customer?.FirstName} {order.Customer?.LastName}".Trim();

            if (string.IsNullOrEmpty(customerName))
            {
                customerName = "Cliente";
            }

            var emailBody = GenerateOrderConfirmationEmailBody(order, customerName);

            var emailMessage = new EmailMessage
            {
                To = customerEmail,
                Subject = $"Confirmação do Pedido #{order.Id.ToString()[..8]}",
                Body = emailBody
            };

            await _emailService.SendEmailAsync(emailMessage, cancellationToken);
            _logger.LogInformation("Order confirmation email sent for order {OrderId} to {Email}", order.Id, customerEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirmation email for order {OrderId}", order.Id);
        }
    }

    private static string GenerateOrderConfirmationEmailBody(Order order, string customerName)
    {
        var itemsHtml = string.Join("", order.Items.Select(item =>
            $@"
            <tr>
                <td style='padding: 12px; border-bottom: 1px solid #e5e7eb;'>{item.Product?.Name ?? "Produto"}</td>
                <td style='padding: 12px; border-bottom: 1px solid #e5e7eb; text-align: center;'>{item.Quantity}</td>
                <td style='padding: 12px; border-bottom: 1px solid #e5e7eb; text-align: right;'>R$ {item.UnitPrice:F2}</td>
                <td style='padding: 12px; border-bottom: 1px solid #e5e7eb; text-align: right;'>R$ {item.Total:F2}</td>
            </tr>"
        ));

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Confirmação do Pedido</title>
</head>
<body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #f8f9fa; padding: 30px; border-radius: 10px; margin-bottom: 20px;'>
        <h1 style='color: #198754; margin: 0; font-size: 28px; text-align: center;'>✓ Pedido Confirmado!</h1>
    </div>
    
    <div style='background-color: white; padding: 30px; border-radius: 10px; box-shadow: 0 2px 10px rgba(0,0,0,0.1);'>
        <h2 style='color: #333; margin-top: 0;'>Olá, {customerName}!</h2>
        
        <p style='font-size: 16px; margin-bottom: 20px;'>
            Seu pedido foi confirmado com sucesso!
        </p>
        
        <div style='background-color: #f8f9fa; padding: 20px; border-radius: 8px; margin: 20px 0;'>
            <h3 style='margin: 0 0 10px 0; color: #495057;'>Detalhes do Pedido</h3>
            <p style='margin: 5px 0;'><strong>Número do Pedido:</strong> #{order.Id.ToString()[..8]}</p>
            <p style='margin: 5px 0;'><strong>Data:</strong> {order.CreatedAt:dd/MM/yyyy HH:mm}</p>
            <p style='margin: 5px 0;'><strong>Status:</strong> {GetStatusInPortuguese(order.Status)}</p>
        </div>
        
        <h3 style='color: #495057; margin: 25px 0 15px 0;'>Itens do Pedido</h3>
        
        <table style='width: 100%; border-collapse: collapse; margin: 20px 0;'>
            <thead>
                <tr style='background-color: #e9ecef;'>
                    <th style='padding: 12px; text-align: left; border-bottom: 2px solid #dee2e6;'>Produto</th>
                    <th style='padding: 12px; text-align: center; border-bottom: 2px solid #dee2e6;'>Qtd</th>
                    <th style='padding: 12px; text-align: right; border-bottom: 2px solid #dee2e6;'>Preço Unit.</th>
                    <th style='padding: 12px; text-align: right; border-bottom: 2px solid #dee2e6;'>Total</th>
                </tr>
            </thead>
            <tbody>
                {itemsHtml}
            </tbody>
        </table>
        
        <div style='text-align: right; margin: 20px 0; padding: 20px; background-color: #f8f9fa; border-radius: 8px;'>
            <h3 style='margin: 0; color: #198754; font-size: 24px;'>Total: R$ {order.Total:F2}</h3>
        </div>
    </div>
    
    <div style='text-align: center; margin-top: 30px; color: #6c757d; font-size: 14px;'>
        <p>Este é um e-mail automático, por favor não responda.</p>
        <p>© 2025 E-commerce App. Todos os direitos reservados.</p>
    </div>
</body>
</html>";
    }

    private static string GetStatusInPortuguese(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Pending => "Pendente",
            OrderStatus.PaymentPending => "Aguardando Pagamento",
            OrderStatus.PaymentProcessing => "Processando Pagamento",
            OrderStatus.PaymentFailed => "Pagamento Falhou",
            OrderStatus.Confirmed => "Confirmado",
            OrderStatus.Canceled => "Cancelado",
            OrderStatus.Shipped => "Enviado",
            OrderStatus.Delivered => "Entregue",
            _ => status.ToString()
        };
    }
}
