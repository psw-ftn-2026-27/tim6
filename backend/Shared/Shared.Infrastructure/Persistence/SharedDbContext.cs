using Microsoft.EntityFrameworkCore;
using Shared.Application;
using Shared.Domain.Images;
using Shared.Infrastructure.Persistence.Images;

namespace Shared.Infrastructure.Persistence;

internal sealed class SharedDbContext : DbContext, IUnitOfWork
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

    public DbSet<Image> Images => Set<Image>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("shared");
        builder.ConfigureImages();
    }
}
