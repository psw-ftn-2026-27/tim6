using AutoMapper;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengeAuthoring;

public sealed class SocialChallengeAuthoringService
{
    private readonly ISocialChallengeRepository _challengeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SocialChallengeAuthoringService(ISocialChallengeRepository challengeRepository, IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _challengeRepository = challengeRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<SocialChallengeDto> CreateAsync(CreateSocialChallengeDto dto)
    {
        var timeWindow = new DailyTimeWindow(dto.TimeWindow.Start, dto.TimeWindow.End);
        var location = dto.Location is null ? null : new Location(dto.Location.Latitude, dto.Location.Longitude);
        var challenge = new SocialChallenge(dto.Name, timeWindow, dto.RequiredPeople, dto.RangeInMeters, dto.Xp,
            dto.ExitMessage, location);

        _challengeRepository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<SocialChallengeDto>(challenge);
    }

    public async Task EditAsync(Guid challengeId, UpdateSocialChallengeDto dto)
    {
        var challenge = await GetChallengeAsync(challengeId);

        var timeWindow = new DailyTimeWindow(dto.TimeWindow.Start, dto.TimeWindow.End);
        var location = dto.Location is null ? null : new Location(dto.Location.Latitude, dto.Location.Longitude);
        challenge.Edit(dto.Name, timeWindow, dto.RequiredPeople, dto.RangeInMeters, dto.Xp, dto.ExitMessage,
            location);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid challengeId)
    {
        var challenge = await GetChallengeAsync(challengeId);

        challenge.Delete();
        _challengeRepository.Remove(challenge);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid challengeId)
    {
        var challenge = await _challengeRepository.GetByIdAsync(challengeId);
        if (challenge is null)
        {
            throw new NotFoundException($"Social challenge {challengeId} does not exist.");
        }
        return challenge;
    }
}
