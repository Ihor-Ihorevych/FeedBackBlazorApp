using Ardalis.Result;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Entities;

namespace FB_App.Application.Movies.Queries.GetMovieById;

public record GetMovieByIdQuery(Guid Id) : IRequest<Result<MovieDetailDto>>;

public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, Result<MovieDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    public GetMovieByIdQueryHandler(
        IApplicationDbContext context,
        IMapper mapper,
        ICacheService cache)
    {
        _context = context;
        _mapper = mapper;
        _cache = cache;
    }

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

        if (movie == null)
        {
            return Result<MovieDetailDto>.NotFound($"{nameof(Movie)} ({request.Id}) was not found.");
        }

        return Result<MovieDetailDto>.Success(movie);
    }
}
