using AutoMapper;
using Games.Application.SocialChallengeAuthoring;
using Games.Domain.SocialChallenges;

namespace Games.Application;

public sealed class GamesMapperProfile : Profile
{
    public GamesMapperProfile()
    {
        CreateMap<SocialChallenge, SocialChallengeDto>();
        CreateMap<DailyTimeWindow, DailyTimeWindowDto>();
        CreateMap<Location, LocationDto>();
    }
}
