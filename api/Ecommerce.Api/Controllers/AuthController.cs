using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Security.Claims;
using Ecommerce.Application.Auth.Commands;
using Ecommerce.Application.Auth.Queries;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator)
    {
        _mediator = mediator;
    }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                message = "Registration successful"
            });
        }
        catch (Exception ex)
        {
            // Log the exception here if you have a logger
            return StatusCode(500, new { message = "An internal server error occurred", details = ex.Message });
        }
    }
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new
            {
                token = result.Token,
                refreshToken = result.RefreshToken,
                expiresAt = result.ExpiresAt,
                message = "Login successful"
            });
        }
        catch (Exception ex)
        {
            // Log the exception here if you have a logger
            return StatusCode(500, new { message = "An internal server error occurred", details = ex.Message });
        }
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var profile = await _mediator.Send(new GetUserProfileQuery(userId));
        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }

    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logout successful" });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return Ok(new { message = "If an account with this email exists, you will receive a password reset email shortly." });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result)
        {
            return BadRequest(new { message = "Invalid or expired reset token" });
        }

        return Ok(new { message = "Password reset successful" });
    }

    [HttpGet("orders")]
    [Authorize]
    public async Task<IActionResult> GetUserOrders()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var orders = await _mediator.Send(new GetUserOrdersQuery(userId));
        return Ok(orders);
    }
}
