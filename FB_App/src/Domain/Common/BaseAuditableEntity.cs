namespace FB_App.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}

/// <summary>
/// Generic auditable entity for entities with custom ID types.
/// </summary>
/// <typeparam name="TId">The type of the entity ID.</typeparam>
public abstract class BaseAuditableEntity<TId> : BaseEntity<TId> where TId : notnull
{
    public DateTimeOffset Created { get; set; }

    public string? CreatedBy { get; set; }

    public DateTimeOffset LastModified { get; set; }

    public string? LastModifiedBy { get; set; }
}
