using Exploration.Application;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence.TourExecutions;
using Exploration.Infrastructure.Persistence.TourPreferences;
using Exploration.Infrastructure.Persistence.Tours;
using Exploration.Infrastructure.Persistence.Conversations;
using Shared.Domain.Conversations;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationDbContext : DbContext, IUnitOfWork
{
    public ExplorationDbContext(DbContextOptions<ExplorationDbContext> options) : base(options) { }

    public DbSet<Tour> Tours => Set<Tour>();
    public DbSet<TourPreference> TourPreferences => Set<TourPreference>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<TourExecution> TourExecutions => Set<TourExecution>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema("exploration");
        builder.ConfigureTours();
        builder.ConfigureTourPreferences();
        builder.ConfigureConversations();
        builder.ConfigureTourExecutions();
    }
}
