using FB_App.Domain.Entities;
using FB_App.Domain.Enums;

namespace FB_App.Application.Movies.Queries.GetMovieById;

public class MovieDetailDtoMappingProfile : Profile
{
    public MovieDetailDtoMappingProfile()
    {
        CreateMap<Movie, MovieDetailDto>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => (Guid)s.Id))
            .ForMember(d => d.ApprovedComments, opt =>
                opt.MapFrom(s => s.Comments
                    .Where(c => c.Status == CommentStatus.Approved)
                    .OrderByDescending(c => c.Id)));
    }
}
