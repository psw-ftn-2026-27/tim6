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

    public async Task<UserSummaryDto?> GetUserByIdAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        return user is null ? null : new UserSummaryDto(user.Id, user.Email);
    }

    public Task<Dictionary<Guid, UserSummaryDto>> GetUsersByIdsAsync(IEnumerable<Guid> ids)
    {
        var uniqueIds = ids.Distinct().ToList();
        
        return _userManager.Users
            .AsNoTracking()
            .Where(user => uniqueIds.Contains(user.Id))
            .ToDictionaryAsync(
                user => user.Id,
                user => new UserSummaryDto(user.Id, user.Email));
    }

    public async Task<UserSummaryDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null ? null : new UserSummaryDto(user.Id, user.Email);
    }

    public async Task<Dictionary<string, UserSummaryDto>> GetUsersByEmailsAsync(IEnumerable<string> emails)
    {
        var normalizedEmails = emails
            .Select(email => _userManager.NormalizeEmail(email))
            .Distinct()
            .ToList();
        
        var users = await _userManager.Users
            .AsNoTracking()
            .Where(user => user.NormalizedEmail != null && normalizedEmails.Contains(user.NormalizedEmail))
            .Select(user => new UserSummaryDto(user.Id, user.Email))
            .ToListAsync();

        var results = new Dictionary<string, UserSummaryDto>(StringComparer.OrdinalIgnoreCase);
        foreach (var user in users)
            if (user.Email is not null)
                results[user.Email] = user;
        
        return results;
    }
}
