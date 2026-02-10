using AutoMapper;
using FB_App.Domain.Entities;
using FB_App.Domain.Enums;

namespace FB_App.Application.Movies.Queries.GetMovies;

public class MovieDtoMappingProfile : Profile
{
    public MovieDtoMappingProfile()
    {
        CreateMap<Movie, MovieDto>()
            .ForMember(d => d.ApprovedCommentsCount, opt => 
                opt.MapFrom(s => s.Comments.Count(c => c.Status == CommentStatus.Approved)));
    }
}
