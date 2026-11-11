using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shared.Domain.Exceptions;
using Social.Application;
using Social.Domain.Blogs;
using Social.Domain.FollowRelationships;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.FollowRelationships;

namespace Social.Infrastructure.Persistence;

internal sealed class SocialDbContext : DbContext, IUnitOfWork
{
    public SocialDbContext(DbContextOptions<SocialDbContext> options) : base(options) { }

    public DbSet<Blog> Blogs => Set<Blog>();
    public DbSet<FollowRelationship> FollowRelationships => Set<FollowRelationship>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new DomainException("The data was changed by another request. Reload and try again.");
        }
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("social");
        builder.ConfigureBlogs();
        builder.ConfigureFollowRelationships();
    }
}
