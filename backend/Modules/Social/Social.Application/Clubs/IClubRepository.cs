using Social.Domain.Clubs;

namespace Social.Application.Clubs;

public interface IClubRepository
{
    Task<Club?> GetByIdAsync(Guid id);
    void Add(Club club);
    void Remove(Club club);
}
