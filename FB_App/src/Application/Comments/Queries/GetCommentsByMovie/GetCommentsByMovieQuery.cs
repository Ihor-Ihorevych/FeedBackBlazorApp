using FB_App.Application.Common.Interfaces;
using FB_App.Domain.Constants;
using FB_App.Domain.Enums;

namespace FB_App.Application.Comments.Queries.GetCommentsByMovie;


public sealed record GetCommentsByMovieQuery : IRequest<List<CommentDetailDto>>
{
    public Guid MovieId { get; init; }
    public CommentStatus? Status { get; init; }
}

public sealed class GetCommentsByMovieQueryHandler : IRequestHandler<GetCommentsByMovieQuery, List<CommentDetailDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IIdentityService _identityService;
    private readonly IUser _user;

    public GetCommentsByMovieQueryHandler(IApplicationDbContext context, IMapper mapper, IIdentityService identityService, IUser user)
    {
        _context = context;
        _mapper = mapper;
        _identityService = identityService;
        _user = user;
    }

    public async Task<List<CommentDetailDto>> Handle(GetCommentsByMovieQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Comments
            .AsNoTracking()
            .Where(c => c.MovieId == request.MovieId);

        var isAdmin = _user.Roles?.Contains(Roles.Administrator) ?? false;
        var currentUserId = _user.Id;

        if (!isAdmin)
        {
            if (string.IsNullOrWhiteSpace(currentUserId))
            {
                query = query.Where(c => c.Status == CommentStatus.Approved);
            }
            else
            {
                query = query.Where(c => c.Status == CommentStatus.Approved ||
                                         (c.Status == CommentStatus.Pending && c.UserId == currentUserId));
            }
        }

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
