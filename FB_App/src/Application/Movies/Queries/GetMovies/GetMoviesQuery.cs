using AutoMapper;
using AutoMapper.QueryableExtensions;
using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Mappings;
using FB_App.Application.Common.Models;
using FB_App.Domain.Enums;

namespace FB_App.Application.Movies.Queries.GetMovies;

public record GetMoviesQuery : IRequest<PaginatedList<MovieDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SearchTerm { get; init; }
    public string? Genre { get; init; }
}

public class GetMoviesQueryHandler : IRequestHandler<GetMoviesQuery, PaginatedList<MovieDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetMoviesQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MovieDto>> Handle(GetMoviesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Movies.AsQueryable();
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchToLower = request.SearchTerm.ToLower();
            query = query.Where(m => 
                m.Title.ToLower().Contains(searchToLower) || 
                (m.Description != null && m.Description.ToLower().Contains(searchToLower)));
        }

        if (!string.IsNullOrWhiteSpace(request.Genre))
        {
            query = query.Where(m => m.Genre != null && m.Genre.ToLower().Contains(request.Genre.ToLower()));
        }

        return await query
            .OrderByDescending(m => m.Id)
            .ProjectTo<MovieDto>(_mapper.ConfigurationProvider)
            .PaginatedListAsync(request.PageNumber, request.PageSize, cancellationToken: cancellationToken);
    }
}
