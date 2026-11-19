using Shared.Domain;
using Social.Domain.Blogs;

namespace Social.Application.Blogs;

public interface IBlogReadRepository
{
    Task<BlogDto?> GetByIdAsync(Guid id);
    Task<List<BlogDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<BlogDto>> GetPublishedAsync(int page, int pageSize);
    Task<VoteSign?> GetVoteSignAsync(Guid blogId, Guid userId);
}
