namespace FB_App.Domain.Common;

/// <summary>
/// Generic auditable entity for entities with custom ID types.
/// </summary>
/// <typeparam name="TId">The type of the entity ID.</typeparam>
public abstract class BaseAuditableEntity<TId> : BaseEntity<TId>, IBaseAuditableEntity where TId : notnull
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}


public interface IBaseAuditableEntity
{
    DateTimeOffset Created { get; set; }
    string? CreatedBy { get; set; }
    DateTimeOffset LastModified { get; set; }
    string? LastModifiedBy { get; set; }
}
