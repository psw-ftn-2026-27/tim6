using Identity.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Domain.Blogs;

namespace Social.Application.BlogReading;

public sealed class BlogReadingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IIdentityApi _identityApi;

    public BlogReadingQueries(IBlogReadRepository blogReadRepository, IIdentityApi identityApi)
    {
        _blogReadRepository = blogReadRepository;
        _identityApi = identityApi;
    }

    public async Task<BlogDto> GetByIdAsync(Guid id)
    {
        var blog = await _blogReadRepository.GetByIdAsync(id);
        if (blog is null || blog.Status == BlogStatus.Draft)
        {
            throw new NotFoundException($"Blog {id} does not exist.");
        }

        var blogs = await AddCommentEmailsAsync([blog]);
        return blogs.Single();
    }

    public async Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var blogs = await _blogReadRepository.GetPublishedAsync(page, pageSize);
        var items = await AddCommentEmailsAsync(blogs.Items);
        return new PageResult<BlogDto>(items, blogs.TotalCount);
    }

    private async Task<List<BlogDto>> AddCommentEmailsAsync(IEnumerable<BlogDto> blogs)
    {
        var users = await _identityApi.GetUsersAsync(
            blogs.SelectMany(blog => blog.Comments).Select(comment => comment.UserId));
        var emails = users.ToDictionary(user => user.UserId, user => user.Email);

        return blogs
            .Select(blog => blog with
            {
                Comments = blog.Comments
                    .Select(comment => comment with { UserEmail = emails[comment.UserId] })
                    .ToList()
            })
            .ToList();
    }
}
