using Microsoft.EntityFrameworkCore;
using Shared.Application;
using Shared.Domain.Images;
using Shared.Domain.Positions;
using Shared.Infrastructure.Persistence.Images;
using Shared.Infrastructure.Persistence.Positions;

namespace Shared.Infrastructure.Persistence;

internal sealed class SharedDbContext : DbContext, IUnitOfWork
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options) { }

    public DbSet<Image> Images => Set<Image>();
    public DbSet<ExplorerPosition> ExplorerPositions => Set<ExplorerPosition>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("shared");
        builder.ConfigureImages();
        builder.ConfigureExplorerPositions();
    }
}
