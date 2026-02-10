namespace FB_App.Domain.Entities.Values;

/// <summary>
/// Strongly-typed identifier for Movie aggregate root.
/// This value object prevents primitive obsession and provides type safety.
/// </summary>
public sealed class MovieId : ValueObject
{
    public int Value { get; }

    private MovieId()
    {
    }

    private MovieId(int value)
    {
        if (value <= 0)
            throw new ArgumentException("MovieId must be greater than zero.", nameof(value));
        
        Value = value;
    }

    /// <summary>
    /// Creates a new MovieId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <returns>A new MovieId instance.</returns>
    /// <exception cref="ArgumentException">Thrown when value is less than or equal to zero.</exception>
    public static MovieId Create(int value) => new(value);

    /// <summary>
    /// Attempts to create a MovieId with the specified value.
    /// </summary>
    /// <param name="value">The identifier value.</param>
    /// <param name="movieId">The created MovieId instance if successful; null otherwise.</param>
    /// <returns>True if the MovieId was created successfully; otherwise false.</returns>
    public static bool TryCreate(int value, out MovieId? movieId)
    {
        movieId = null;
        if (value <= 0)
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
    /// Implicitly converts a MovieId to its int value.
    /// </summary>
    public static implicit operator int(MovieId movieId) => movieId.Value;

    /// <summary>
    /// Explicitly converts an int to a MovieId.
    /// </summary>
    public static explicit operator MovieId(int value) => Create(value);
}
