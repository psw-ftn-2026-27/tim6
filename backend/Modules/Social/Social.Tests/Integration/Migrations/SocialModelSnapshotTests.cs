using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Social.Infrastructure.Persistence;
using Xunit;

namespace Social.Tests.Integration.Migrations;

/// <summary>
/// Guards against changing the model without adding a migration. The check compares the model with the
/// latest snapshot and never opens a connection, so the connection string below is a placeholder.
/// </summary>
public class SocialModelSnapshotTests
{
    [Fact]
    public void The_model_has_no_changes_that_are_missing_from_the_migrations()
    {
        var options = new DbContextOptionsBuilder<SocialDbContext>()
            .UseNpgsql("Host=localhost;Database=never-opened",
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "social"))
            .Options;
        using var context = new SocialDbContext(options);

        context.Database.HasPendingModelChanges().Should().BeFalse(
            "a model change needs a migration, and the snapshot must be regenerated with it");
    }
}
