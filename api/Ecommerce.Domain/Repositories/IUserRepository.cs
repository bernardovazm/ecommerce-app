using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<Customer?> GetCustomerByIdAsync(Guid id);
    Task<Customer?> GetCustomerByEmailAsync(string email);
    Task<User> AddAsync(User user);
    Task<Customer> AddCustomerAsync(Customer customer);
    Task UpdateAsync(User user);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string email);
    Task SaveChangesAsync();
}
