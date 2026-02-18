namespace FB_App.Domain.Entities.Values;

/// <summary>
/// Strongly-typed identifier for Movie aggregate root.
/// This value object prevents primitive obsession and provides type safety.
/// </summary>
public sealed class MovieId : ValueObject
{
    public Guid Value { get; }

    private MovieId()
    {
    }

    private MovieId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("MovieId cannot be empty.", nameof(value));

        Value = value;
    }

    /// <summary>
    /// Creates a new MovieId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <returns>A new MovieId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is empty.</exception>
    public static MovieId Create(Guid value) => new(value);

    /// <summary>
    /// Creates a new MovieId with a newly generated GUID value.
    /// </summary>
    /// <returns>A new MovieId instance with a generated GUID.</returns>
    public static MovieId CreateNew() => new(Guid.CreateVersion7());

    /// <summary>
    /// Attempts to create a MovieId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <param name="movieId">The created MovieId instance if successful; null otherwise.</param>
    /// <returns>True if the MovieId was created successfully; otherwise false.</returns>
    public static bool TryCreate(Guid value, out MovieId? movieId)
    {
        movieId = null;
        if (value == Guid.Empty)
            return false;

        movieId = new MovieId(value);
        return true;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString();

    /// <summary>
    /// Implicitly converts a MovieId to its Guid value.
    /// </summary>
    public static implicit operator Guid(MovieId movieId) => movieId.Value;

    /// <summary>
    /// Explicitly converts a Guid to a MovieId.
    /// </summary>
    public static explicit operator MovieId(Guid value) => new(value);


    public override bool Equals(object? obj)
    {
        return obj switch
        {
            MovieId other => Value == other.Value,
            _ => false
        };
    }

    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(MovieId? left, MovieId? right)
    {
        return left switch
        {
            null => right is null,
            _ => left.Equals(right)
        };
    }

    public static bool operator !=(MovieId? left, MovieId? right)
    {
        return !(left == right);
    }
}
