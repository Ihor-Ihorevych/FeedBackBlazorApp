using System.ComponentModel.DataAnnotations.Schema;

namespace FB_App.Domain.Common;

public abstract class BaseEntity : BaseEntity<int>;

/// <summary>
/// Generic base entity for entities with custom ID types.
/// </summary>
/// <typeparam name="TId">The type of the entity ID.</typeparam>
public abstract class BaseEntity<TId> where TId : notnull
{
    public abstract TId Id { get; }

    private readonly List<BaseEvent> _domainEvents = new();

    [NotMapped]
    public IReadOnlyCollection<BaseEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(BaseEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
