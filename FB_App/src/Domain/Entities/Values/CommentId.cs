namespace FB_App.Domain.Entities.Values;

/// <summary>
/// Strongly-typed identifier for Comment entity within Movie aggregate.
/// This value object prevents primitive obsession and provides type safety.
/// </summary>
public sealed class CommentId : ValueObject
{
    public Guid Value { get; }

    private CommentId()
    {
    }

    private CommentId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("CommentId cannot be empty.", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// Creates a new CommentId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <returns>A new CommentId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is empty.</exception>
    public static CommentId Create(Guid value) => new(value);

    /// <summary>
    /// Creates a new CommentId with a newly generated GUID value.
    /// </summary>
    /// <returns>A new CommentId instance with a generated GUID.</returns>
    public static CommentId CreateNew() => new(Guid.CreateVersion7());

    /// <summary>
    /// Attempts to create a CommentId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <param name="commentId">The created CommentId instance if successful; null otherwise.</param>
    /// <returns>True if the CommentId was created successfully; otherwise false.</returns>
    public static bool TryCreate(Guid value, out CommentId? commentId)
    {
        commentId = null;
        if (value == Guid.Empty)
            return false;

        commentId = new CommentId(value);
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Implicitly converts a CommentId to its Guid value.
    /// </summary>
    public static implicit operator Guid(CommentId commentId) => commentId.Value;

    /// <summary>
    /// Explicitly converts a Guid to a CommentId.
    /// </summary>
    public static explicit operator CommentId(Guid value) => new(value);

    /// <summary>
    /// Determines whether two CommentId instances are equal.
    /// </summary>
    public static bool operator ==(CommentId? left, CommentId? right)
    {
        if (left is null)
            return right is null;
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether two CommentId instances are not equal.
    /// </summary>
    public static bool operator !=(CommentId? left, CommentId? right) => !(left == right);

    public override bool Equals(object? obj)
    {
        if (obj is CommentId other)
            return Value == other.Value;
        return false;
    }

    public override int GetHashCode() => Value.GetHashCode();
}
