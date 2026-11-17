using Identity.Contracts;
using Social.Application.Blogs;

namespace Social.Application.BlogAuthoring;

public sealed class BlogAuthoringQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IIdentityApi _identityApi;

    public BlogAuthoringQueries(IBlogReadRepository blogReadRepository, IIdentityApi identityApi)
    {
        _blogReadRepository = blogReadRepository;
        _identityApi = identityApi;
    }

    public async Task<List<BlogDto>> GetByAuthorAsync(Guid authorId)
    {
        var blogs = await _blogReadRepository.GetByAuthorAsync(authorId);
        var users = await _identityApi.GetUsersAsync(
            blogs.SelectMany(blog => blog.Comments).Select(comment => comment.UserId));
        var emails = users.ToDictionary(user => user.UserId, user => user.Email);

        return blogs
                     .Select(blog => blog with
                     {
                         Comments = blog.Comments
                             .Select(comment => comment with
                             {
                                 UserEmail = emails.TryGetValue(comment.UserId, out var email)
                                     ? email
                                     : null
                             })
                             .ToList()
                     })
                     .ToList();
    }
}
