using Shared.Domain.Exceptions;
using Social.Application.Blogs;
using Social.Application.FollowRelationships;
using Social.Domain.Blogs;
using Social.Domain.FollowRelationships;

namespace Social.Application.Following;

public sealed class FollowingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;
    private readonly IFollowRelationshipRepository _relationshipRepository;
    private readonly IFollowRelationshipReadRepository _relationshipReadRepository;

    public FollowingQueries(
        IBlogReadRepository blogReadRepository,
        IFollowRelationshipRepository relationshipRepository,
        IFollowRelationshipReadRepository relationshipReadRepository)
    {
        _blogReadRepository = blogReadRepository;
        _relationshipRepository = relationshipRepository;
        _relationshipReadRepository = relationshipReadRepository;
    }

    public async Task<BlogFollowStatusDto> GetBlogFollowStatusAsync(Guid userId, Guid blogId)
    {
        var blog = await _blogReadRepository.GetByIdAsync(blogId);
        if (blog is null || blog.Status != BlogStatus.Published)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }

        if (blog.AuthorId == userId)
        {
            return new BlogFollowStatusDto(BlogFollowState.OwnBlog);
        }

        var relationship = await _relationshipRepository.GetByPairAsync(userId, blog.AuthorId);
        if (relationship is null)
        {
            return new BlogFollowStatusDto(BlogFollowState.Follow);
        }

        var state = relationship.GetFollowStatusFor(userId) switch
        {
            FollowStatus.Following => BlogFollowState.Following,
            FollowStatus.Pending => BlogFollowState.Pending,
            _ => BlogFollowState.Follow
        };

        return new BlogFollowStatusDto(state);
    }

    public Task<List<SentFollowRequestDto>> GetSentRequestsAsync(Guid userId)
    {
        return _relationshipReadRepository.GetSentRequestsAsync(userId);
    }

    public Task<List<FollowedUserDto>> GetFollowedUsersAsync(Guid userId)
    {
        return _relationshipReadRepository.GetFollowedUsersAsync(userId);
    }
}
