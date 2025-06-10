using MediatR;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Domain.Models;

namespace Ecommerce.Application.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<bool>;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailService _emailService;

    public ForgotPasswordCommandHandler(
        IUserRepository userRepository, 
        IPasswordResetService passwordResetService,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal if user exists or not for security
            return true;
        }

        var resetToken = await _passwordResetService.GenerateResetTokenAsync(user.Id);
        
        // Send reset email
        var resetLink = $"http://localhost:5174/reset-password?token={resetToken}";
        var emailMessage = new EmailMessage
        {
            To = user.Email,
            Subject = "Password Reset Request",
            Body = $@"
                <h2>Password Reset Request</h2>
                <p>Hi {user.FirstName},</p>
                <p>You requested a password reset. Click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>This link will expire in 24 hours.</p>
                <p>If you didn't request this, please ignore this email.</p>
            "
        };

        await _emailService.SendEmailAsync(emailMessage);
        return true;
    }
}
