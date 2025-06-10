
namespace Ecommerce.Domain.Entities;
using Ardalis.GuardClauses;

public class Customer : User
{
    public string Phone { get; private set; } = string.Empty;
    public string ShippingAddress { get; private set; } = string.Empty;
    
    private readonly List<Order> _orders = new();
    public IReadOnlyCollection<Order> Orders => _orders;

    protected Customer() { }

    public Customer(string email, string passwordHash, string firstName, string lastName, string phone = "") 
        : base(email, passwordHash, firstName, lastName)
    {
        Phone = phone;
    }

    public void UpdatePhone(string phone)
    {
        Phone = phone ?? string.Empty;
    }

    public void UpdateShippingAddress(string address)
    {
        ShippingAddress = address ?? string.Empty;
    }

    public void AddOrder(Order order)
    {
        Guard.Against.Null(order);
        _orders.Add(order);
    }
}