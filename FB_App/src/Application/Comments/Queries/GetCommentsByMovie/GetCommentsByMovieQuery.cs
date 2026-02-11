using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Enums;

namespace FB_App.Application.Comments.Queries.GetCommentsByMovie;


[Authorize(Roles = Roles.Administrator)]
public record GetCommentsByMovieQuery : IRequest<List<CommentDetailDto>>
{
    public Guid MovieId { get; init; }
    public CommentStatus? Status { get; init; }
}

public class GetCommentsByMovieQueryHandler : IRequestHandler<GetCommentsByMovieQuery, List<CommentDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetCommentsByMovieQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<CommentDetailDto>> Handle(GetCommentsByMovieQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.MovieId == request.MovieId);

        if (request.Status.HasValue)
        {
            query = query.Where(c => c.Status == request.Status.Value);
        }

        return await query
            .OrderByDescending(c => c.Id)
            .ProjectTo<CommentDetailDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);
    }
}
