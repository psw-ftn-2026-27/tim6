namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<UserSummaryDto> GetUserAsync(Guid userId);
    Task<List<UserSummaryDto>> GetUsersAsync(IEnumerable<Guid> userIds);
}
