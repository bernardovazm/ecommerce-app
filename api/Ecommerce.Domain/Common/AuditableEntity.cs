namespace Ecommerce.Domain.Common;

public abstract class AuditableEntity<TId>
{
    public TId Id { get; protected set; } = default!;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public void SetUpdatedNow() => UpdatedAt = DateTime.UtcNow;
}
