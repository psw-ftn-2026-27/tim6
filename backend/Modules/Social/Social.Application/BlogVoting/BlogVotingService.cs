using Shared.Domain.Exceptions;
using Social.Application.Blogs;

namespace Social.Application.BlogVoting;

public sealed class BlogVotingService
{
    private readonly IBlogRepository _blogRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BlogVotingService(IBlogRepository blogRepository, IUnitOfWork unitOfWork)
    {
        _blogRepository = blogRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task CastVoteAsync(Guid blogId, Guid userId, CastVoteDto dto)
    {
        var blog = await _blogRepository.GetByIdAsync(blogId);
        if (blog is null)
        {
            throw new NotFoundException($"Blog {blogId} does not exist.");
        }

        var now = DateTime.UtcNow;

        blog.CastVote(userId, dto.Sign, now);
        await _unitOfWork.SaveChangesAsync();
    }
}
