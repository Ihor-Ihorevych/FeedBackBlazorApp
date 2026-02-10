namespace FB_App.Domain.Entities.Values;

/// <summary>
/// Strongly-typed identifier for Comment entity within Movie aggregate.
/// This value object prevents primitive obsession and provides type safety.
/// </summary>
public sealed class CommentId : ValueObject
{
    public int Value { get; }

    private CommentId()
    {
    }

    private CommentId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("CommentId must be greater than zero.", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// Creates a new CommentId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <returns>A new CommentId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is less than or equal to zero.</exception>
    public static CommentId Create(int value) => new(value);

    /// <summary>
    /// Attempts to create a CommentId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <param name="commentId">The created CommentId instance if successful; null otherwise.</param>
    /// <returns>True if the CommentId was created successfully; otherwise false.</returns>
    public static bool TryCreate(int value, out CommentId? commentId)
    {
        commentId = null;
        if (value <= 0)
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
    /// Implicitly converts a CommentId to its int value.
    /// </summary>
    public static implicit operator int(CommentId commentId) => commentId.Value;

    /// <summary>
    /// Explicitly converts an int to a CommentId.
    /// </summary>
    public static explicit operator CommentId(int value) => Create(value);
}
