using AutoMapper;
using FB_App.Domain.Entities;

namespace FB_App.Application.Comments.Queries;

public class CommentDtoMappingProfile : Profile
{
    public CommentDtoMappingProfile()
    {
        CreateMap<Comment, CommentDto>();
    }
}
