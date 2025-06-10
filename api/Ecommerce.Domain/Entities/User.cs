
namespace Ecommerce.Domain.Entities;
using Ecommerce.Domain.Common;
using Ardalis.GuardClauses;

public class User : AuditableEntity<Guid>
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsEmailConfirmed { get; private set; } = false;
    public DateTime? LastLoginAt { get; private set; }
    public UserRole Role { get; private set; } = UserRole.Customer;

    protected User() { }

    public User(string email, string passwordHash, string firstName, string lastName)
    {
        Guard.Against.NullOrWhiteSpace(email);
        Guard.Against.NullOrWhiteSpace(passwordHash);
        Guard.Against.NullOrWhiteSpace(firstName);
        Guard.Against.NullOrWhiteSpace(lastName);

        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        Guard.Against.NullOrWhiteSpace(newPasswordHash);
        PasswordHash = newPasswordHash;
    }

    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void UpdateProfile(string firstName, string lastName)
    {
        Guard.Against.NullOrWhiteSpace(firstName);
        Guard.Against.NullOrWhiteSpace(lastName);
        
        FirstName = firstName;
        LastName = lastName;
    }

    public string FullName => $"{FirstName} {LastName}";
}

public enum UserRole
{
    Customer = 0,
    Admin = 1
}