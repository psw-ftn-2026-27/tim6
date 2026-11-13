using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Domain.Exceptions;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.Clubs;
using Social.Domain.Following;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Clubs;
using Social.Infrastructure.Persistence.Following;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    /// <summary>
    /// Unique indexes that guard follow rules against two requests racing past the application checks.
    /// Violations of any other unique index are unexpected and are not translated.
    /// </summary>
    private static readonly string[] FollowRaceIndexes =
    [
        FollowConfiguration.UniqueFollowIndex,
        FollowConfiguration.UniquePendingRequestIndex
    ];

    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<Club> Clubs => Set<Club>();
    public DbSet<Follow> Follows => Set<Follow>();
    public DbSet<FollowRequest> FollowRequests => Set<FollowRequest>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsFollowRaceViolation(exception))
        {
            throw new DomainException("The follow state was changed by another request. Reload and try again.");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureFollowing();
        builder.ConfigureClubs();
    }

    private static bool IsFollowRaceViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: { } constraintName
        } && FollowRaceIndexes.Contains(constraintName);
}
