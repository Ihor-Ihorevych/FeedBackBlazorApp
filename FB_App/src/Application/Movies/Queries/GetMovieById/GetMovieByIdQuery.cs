using Ardalis.Result;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Entities;

namespace FB_App.Application.Movies.Queries.GetMovieById;

public record GetMovieByIdQuery(Guid Id) : IRequest<Result<MovieDetailDto>>;

public class GetMovieByIdQueryHandler : IRequestHandler<GetMovieByIdQuery, Result<MovieDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMovieByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<MovieDetailDto>> Handle(GetMovieByIdQuery request, CancellationToken cancellationToken)
    {
        var movie = await _context.Movies
            .ProjectTo<MovieDetailDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken);

        if (movie == null)
        {
            return Result<MovieDetailDto>.NotFound($"{nameof(Movie)} ({request.Id}) was not found.");
        }

        return Result<MovieDetailDto>.Success(movie);
    }
}
