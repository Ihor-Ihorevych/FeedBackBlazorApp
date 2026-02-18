using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Entities;

namespace FB_App.Application.Movies.Queries.GetMovieById;

public sealed record GetMovieByIdQuery(Guid Id) : IRequest<Result<MovieDetailDto>>;

public sealed class GetMovieByIdQueryHandler(
    IApplicationDbContext context,
    IMapper mapper,
    ICacheService cache) : IRequestHandler<GetMovieByIdQuery, Result<MovieDetailDto>>
{
    private readonly IApplicationDbContext _context = context;
    private readonly IMapper _mapper = mapper;
    private readonly ICacheService _cache = cache;

    public async Task<Result<MovieDetailDto>> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.MovieById(request.Id);

        var movie = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct => await _context.Movies
                .ProjectTo<MovieDetailDto>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync(m => m.Id == request.Id, ct),
            TimeSpan.FromMinutes(5),
            cancellationToken);

        return movie switch
        {
            null => Result<MovieDetailDto>.NotFound($"{nameof(Movie)} ({request.Id}) was not found."),
            _ => Result<MovieDetailDto>.Success(movie)
        };
    }
}
