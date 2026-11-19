namespace Identity.Contracts;

public sealed record UserSearchPageDto(List<UserSummaryDto> Items, int TotalCount);
