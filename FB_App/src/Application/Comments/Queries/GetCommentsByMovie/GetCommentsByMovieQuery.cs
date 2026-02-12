using FB_App.Application.Common.Interfaces;
using FB_App.Application.Common.Security;
using FB_App.Domain.Constants;
using FB_App.Domain.Enums;

namespace FB_App.Application.Comments.Queries.GetCommentsByMovie;


public record GetCommentsByMovieQuery : IRequest<List<CommentDetailDto>>
{
    public Guid MovieId { get; init; }
    public CommentStatus? Status { get; init; }
}

public class GetCommentsByMovieQueryHandler : IRequestHandler<GetCommentsByMovieQuery, List<CommentDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;

    public GetCommentsByMovieQueryHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
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

        var comments = await query
            .OrderByDescending(c => c.Id)
            .ToListAsync(cancellationToken);

        var result = _mapper.Map<List<CommentDetailDto>>(comments);

        var userNameTasks = result.Where(c => !string.IsNullOrWhiteSpace(c.UserId))
                .Select(async comment =>
                {
                    comment.UserName = await _identityService.GetUserNameAsync(comment.UserId) ?? string.Empty;
                });
        
        await Task.WhenAll(userNameTasks);

        return result;
    }
}
