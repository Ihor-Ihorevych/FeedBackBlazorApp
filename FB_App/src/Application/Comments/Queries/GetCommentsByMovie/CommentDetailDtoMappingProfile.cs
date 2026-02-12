using AutoMapper;
using FB_App.Domain.Entities;

namespace FB_App.Application.Comments.Queries.GetCommentsByMovie;

public class CommentDetailDtoMappingProfile : Profile
{
    public CommentDetailDtoMappingProfile()
    {
        CreateMap<Comment, CommentDetailDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => (Guid)s.Id))
            .ForMember(d => d.MovieId, opt => opt.MapFrom(s => (Guid)s.MovieId))
            .ForMember(d => d.UserName, opt => opt.Ignore());
    }
}
