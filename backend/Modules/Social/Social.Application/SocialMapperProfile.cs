using AutoMapper;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application;

public sealed class SocialMapperProfile : Profile
{
    public SocialMapperProfile()
    {
        CreateMap<Blog, BlogDto>()
            .ForCtorParam(nameof(BlogDto.Score), options => options.MapFrom(_ => 0));
        CreateMap<Comment, CommentDto>();
    }
}
