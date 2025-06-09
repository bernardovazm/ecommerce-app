
namespace Ecommerce.Domain.Entities;
public class Customer : User
{
    public string Name { get; private set; } = string.Empty;
    protected Customer() { }
    public Customer(string name, string email) : base(email) => Name = name;
}