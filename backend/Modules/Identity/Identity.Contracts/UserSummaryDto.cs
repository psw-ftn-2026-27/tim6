namespace Identity.Contracts;

public sealed record UserSummaryDto(Guid UserId, string? Email, bool Exists);
