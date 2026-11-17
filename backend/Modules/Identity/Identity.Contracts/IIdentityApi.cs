namespace Identity.Contracts;

public interface IIdentityApi
{
    Task<UserSummaryDto?> GetUserByIdAsync(Guid userId);
    Task<Dictionary<Guid, UserSummaryDto>> GetUsersByIdsAsync(IEnumerable<Guid> ids);
    Task<UserSummaryDto?> GetUserByEmailAsync(string email);
    Task<Dictionary<string, UserSummaryDto>> GetUsersByEmailsAsync(IEnumerable<string> emails);
}
