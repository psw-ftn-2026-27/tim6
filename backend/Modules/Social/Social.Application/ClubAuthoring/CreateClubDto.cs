namespace Social.Application.ClubAuthoring;

public sealed record CreateClubDto(string Name, string Description, List<string> Images);
