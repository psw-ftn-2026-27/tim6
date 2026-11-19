using Social.Application.Blogs;

namespace Social.Application.BlogVoting;

public sealed class BlogVotingQueries
{
    private readonly IBlogReadRepository _blogReadRepository;

    public BlogVotingQueries(IBlogReadRepository blogReadRepository)
    {
        _blogReadRepository = blogReadRepository;
    }

    public async Task<MyVoteDto> GetMyVoteAsync(Guid blogId, Guid userId)
    {
        return new MyVoteDto(await _blogReadRepository.GetVoteSignAsync(blogId, userId));
    }
}
