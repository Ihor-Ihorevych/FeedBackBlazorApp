using AutoMapper;
using FB_App.Domain.Entities;

namespace FB_App.Application.Comments.Queries.GetCommentsByMovie;

public class CommentDetailDtoMappingProfile : Profile
{
    public CommentDetailDtoMappingProfile()
    {
        CreateMap<Comment, CommentDetailDto>();
    }
}
