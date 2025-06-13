using MediatR;
using Ecommerce.Domain.Repositories;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Auth.Commands;

public record ResetPasswordCommand(string Token, string NewPassword) : IRequest<bool>;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IPasswordService _passwordService;

    public ResetPasswordCommandHandler(
        IUserRepository userRepository,
        IPasswordResetService passwordResetService,
        IPasswordService passwordService)
    {
        _userRepository = userRepository;
        _passwordResetService = passwordResetService;
        _passwordService = passwordService;
    }

    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var userId = await _passwordResetService.ValidateResetTokenAsync(request.Token);
        if (userId == null)
        {
            return false;
        }

        var user = await _userRepository.GetByIdAsync(userId.Value);
        if (user == null)
        {
            return false;
        }

        var hashedPassword = _passwordService.HashPassword(request.NewPassword);
        user.UpdatePassword(hashedPassword);

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        await _passwordResetService.InvalidateResetTokenAsync(request.Token);

        return true;
    }
}
