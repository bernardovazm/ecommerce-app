
namespace Ecommerce.Domain.Entities;
public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Email { get; private set; } = string.Empty;
    protected User() { }
    public User(string email) => Email = email;
}