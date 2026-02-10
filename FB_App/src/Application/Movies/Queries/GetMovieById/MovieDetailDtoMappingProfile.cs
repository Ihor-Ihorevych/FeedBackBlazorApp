using AutoMapper;
using FB_App.Application.Comments.Queries;
using FB_App.Domain.Entities;
using FB_App.Domain.Enums;

namespace FB_App.Application.Movies.Queries.GetMovieById;

public class MovieDetailDtoMappingProfile : Profile
{
    public MovieDetailDtoMappingProfile()
    {
        CreateMap<Movie, MovieDetailDto>()
            .ForMember(d => d.ApprovedComments, opt =>
                opt.MapFrom(s => s.Comments
                    .Where(c => c.Status == CommentStatus.Approved)
                    .OrderByDescending(c => c.Created)));
    }
}
