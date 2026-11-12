using Identity.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Core;

public sealed class IdentityApi : IIdentityApi
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityApi(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<UserSummaryDto> GetUserAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return new UserSummaryDto(userId, user?.Email, user is not null);
    }

    public async Task<List<UserSummaryDto>> GetUsersAsync(IEnumerable<Guid> userIds)
    {
        var uniqueUserIds = userIds.Distinct().ToList();
        var users = await _userManager.Users
            .Where(user => uniqueUserIds.Contains(user.Id))
            .Select(user => new { user.Id, user.Email })
            .ToDictionaryAsync(user => user.Id);

        return uniqueUserIds
            .Select(userId => users.TryGetValue(userId, out var user)
                ? new UserSummaryDto(userId, user.Email, true)
                : new UserSummaryDto(userId, null, false))
            .ToList();
    }
}
