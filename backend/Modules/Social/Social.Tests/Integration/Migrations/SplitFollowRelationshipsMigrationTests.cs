using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using NpgsqlTypes;
using Social.Domain.Following;
using Social.Infrastructure.Persistence;
using Xunit;

namespace Social.Tests.Integration.Migrations;

/// <summary>
/// Runs SplitFollowRelationships against rows written in the previous schema. Every test gets its own
/// throwaway database, brought to the migration before the split, so nothing here touches the shared
/// integration test database. Old rows are inserted with plain SQL because the old model no longer exists;
/// the result is read back through the current model, which also proves the new schema fits it.
/// </summary>
public class SplitFollowRelationshipsMigrationTests : IAsyncLifetime
{
    private const string SplitMigration = "20261113120000_SplitFollowRelationships";

    private static readonly DateTime Created = new(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime Answered = Created.AddMinutes(5);

    // The old model kept one row per pair of users with a flag per direction, and every request of that pair
    // hung off it. First and Second are in no particular relation to who sent what, so the data below covers
    // senders on both sides, a follow in each direction, both together, and none.
    private static readonly OldRelationship[] Relationships =
    [
        new(Id(101), First: Id(1), Second: Id(2), FirstFollowsSecond: true, SecondFollowsFirst: false),
        new(Id(102), First: Id(3), Second: Id(4), FirstFollowsSecond: true, SecondFollowsFirst: true),
        new(Id(103), First: Id(5), Second: Id(6), FirstFollowsSecond: false, SecondFollowsFirst: false),
        new(Id(104), First: Id(7), Second: Id(8), FirstFollowsSecond: false, SecondFollowsFirst: false),
        new(Id(105), First: Id(9), Second: Id(10), FirstFollowsSecond: false, SecondFollowsFirst: false),
        new(Id(106), First: Id(11), Second: Id(12), FirstFollowsSecond: false, SecondFollowsFirst: true)
    ];

    private static readonly OldRequest[] Requests =
    [
        // 1 follows 2: accepted, and the follow is still active.
        new(Id(201), Id(101), Sender: Id(1), Recipient: Id(2), FollowRequestStatus.Accepted, Created, Answered),
        // 3 and 4 follow each other. The second request was accepted the moment it was sent because 3
        // already followed 4, so it was answered at the same instant it was created.
        new(Id(202), Id(102), Sender: Id(3), Recipient: Id(4), FollowRequestStatus.Accepted, Created, Answered),
        new(Id(203), Id(102), Sender: Id(4), Recipient: Id(3), FollowRequestStatus.Accepted,
            Created.AddHours(1), Created.AddHours(1)),
        // 6 asked 5, who is stored as First: the sender is on the Second side.
        new(Id(204), Id(103), Sender: Id(6), Recipient: Id(5), FollowRequestStatus.Pending, Created, null),
        // 7 was turned down twice and has asked 8 a third time: history next to one open request.
        new(Id(205), Id(104), Sender: Id(7), Recipient: Id(8), FollowRequestStatus.Rejected, Created, Answered),
        new(Id(206), Id(104), Sender: Id(7), Recipient: Id(8), FollowRequestStatus.Rejected,
            Created.AddDays(1), Answered.AddDays(1)),
        new(Id(207), Id(104), Sender: Id(7), Recipient: Id(8), FollowRequestStatus.Pending,
            Created.AddDays(2), null),
        // 9 followed 10 and stopped: the accepted request stays, the follow does not.
        new(Id(208), Id(105), Sender: Id(9), Recipient: Id(10), FollowRequestStatus.Accepted, Created, Answered),
        // Only the Second side follows the First side.
        new(Id(209), Id(106), Sender: Id(12), Recipient: Id(11), FollowRequestStatus.Accepted, Created, Answered)
    ];

    private static readonly FollowPair[] ExpectedFollows =
    [
        new(Id(1), Id(2)),
        new(Id(3), Id(4)),
        new(Id(4), Id(3)),
        new(Id(12), Id(11))
    ];

    private readonly NpgsqlConnectionStringBuilder _connection;

    public SplitFollowRelationshipsMigrationTests()
    {
        _connection = new NpgsqlConnectionStringBuilder(
            Environment.GetEnvironmentVariable("EXPLORER_TEST_DATABASE")
                ?? "Host=localhost;Port=5432;Database=explorer-test;Username=postgres;Password=admin");
        _connection.Database += $"-split-follows-{Guid.NewGuid().ToString("N")[..8]}";
    }

    public async Task InitializeAsync()
    {
        await ExecuteOnServerAsync($"CREATE DATABASE \"{_connection.Database}\"");

        await using var context = CreateContext();
        var migrations = context.Database.GetMigrations().ToList();
        var split = migrations.IndexOf(SplitMigration);
        split.Should().BeGreaterThan(0, "the split migration must exist and have a predecessor");

        await context.GetService<IMigrator>().MigrateAsync(migrations[split - 1]);
    }

    public async Task DisposeAsync()
    {
        NpgsqlConnection.ClearAllPools();
        await ExecuteOnServerAsync($"DROP DATABASE IF EXISTS \"{_connection.Database}\" WITH (FORCE)");
    }

    [Fact]
    public async Task Up_turns_every_active_flag_into_one_directed_follow()
    {
        await InsertOldDataAsync(Relationships, Requests);

        await MigrateToAsync(SplitMigration);

        await using var context = CreateContext();
        var follows = await context.Follows.AsNoTracking().ToListAsync();
        follows.Select(follow => new FollowPair(follow.FollowerId, follow.FollowedUserId))
            .Should().BeEquivalentTo(ExpectedFollows);
        follows.Select(follow => follow.Id).Should().OnlyHaveUniqueItems().And.NotContain(Guid.Empty);
    }

    [Fact]
    public async Task Up_keeps_every_request_with_its_sender_recipient_status_and_timestamps()
    {
        await InsertOldDataAsync(Relationships, Requests);

        await MigrateToAsync(SplitMigration);

        await using var context = CreateContext();
        var requests = await context.FollowRequests.AsNoTracking().ToListAsync();
        requests.Select(request => new MigratedRequest(request.Id, request.SenderId, request.RecipientId,
                request.Status, request.CreatedAt, request.RespondedAt))
            .Should().BeEquivalentTo(Requests.Select(request => new MigratedRequest(request.Id, request.Sender,
                request.Recipient, request.Status, request.CreatedAt, request.RespondedAt)));
        requests.Select(request => request.Status).Distinct()
            .Should().BeEquivalentTo(
                [FollowRequestStatus.Pending, FollowRequestStatus.Accepted, FollowRequestStatus.Rejected],
                "the data must exercise every status");
    }

    [Fact]
    public async Task Up_keeps_only_the_oldest_of_several_pending_requests_for_the_same_sender_and_recipient()
    {
        // The old schema had no constraint on this, so two requests that raced past the aggregate's check
        // can both be stored as pending. The new partial unique index would reject them and abort Up.
        var relationship = new OldRelationship(
            Id(107), First: Id(13), Second: Id(14), FirstFollowsSecond: false, SecondFollowsFirst: false);
        await InsertOldDataAsync(
            [relationship],
            [
                new OldRequest(Id(210), Id(107), Sender: Id(13), Recipient: Id(14), FollowRequestStatus.Pending, Created, null),
                new OldRequest(Id(211), Id(107), Sender: Id(13), Recipient: Id(14), FollowRequestStatus.Pending,
                    Created.AddHours(1), null),
                new OldRequest(Id(212), Id(107), Sender: Id(13), Recipient: Id(14), FollowRequestStatus.Pending,
                    Created.AddHours(2), null),
                // Answered requests of the same pair are history, never duplicates.
                new OldRequest(Id(213), Id(107), Sender: Id(13), Recipient: Id(14), FollowRequestStatus.Rejected,
                    Created.AddDays(-1), Answered.AddDays(-1)),
                new OldRequest(Id(217), Id(107), Sender: Id(13), Recipient: Id(14), FollowRequestStatus.Accepted,
                    Created.AddDays(-2), Answered.AddDays(-2)),
                // The opposite direction is a different pair of sender and recipient. Two of them created at the
                // same instant are told apart by Id, so the outcome does not depend on row order.
                new OldRequest(Id(214), Id(107), Sender: Id(14), Recipient: Id(13), FollowRequestStatus.Pending, Created, null),
                new OldRequest(Id(215), Id(107), Sender: Id(14), Recipient: Id(13), FollowRequestStatus.Pending, Created, null)
            ]);

        await MigrateToAsync(SplitMigration);

        await using var context = CreateContext();
        (await context.FollowRequests.AsNoTracking().Select(request => request.Id).ToListAsync())
            .Should().BeEquivalentTo([Id(210), Id(213), Id(214), Id(217)],
                "the oldest pending request per direction stays, and nothing answered is touched");
        (await IndexDefinitionAsync("IX_FollowRequests_SenderId_RecipientId_Pending")).Should().Contain("UNIQUE");
    }

    [Fact]
    public async Task Up_keeps_an_automatically_accepted_request_together_with_the_follows_of_its_pair()
    {
        await InsertOldDataAsync(
            Relationships.Where(relationship => relationship.Id == Id(102)),
            Requests.Where(request => request.RelationshipId == Id(102)));

        await MigrateToAsync(SplitMigration);

        await using var context = CreateContext();
        var automatic = await context.FollowRequests.AsNoTracking().SingleAsync(request => request.Id == Id(203));
        automatic.Status.Should().Be(FollowRequestStatus.Accepted);
        automatic.RespondedAt.Should().Be(automatic.CreatedAt, "it was answered at the moment it was sent");
        (await context.FollowRequests.CountAsync()).Should().Be(2);
        (await context.Follows.AsNoTracking().ToListAsync())
            .Select(follow => new FollowPair(follow.FollowerId, follow.FollowedUserId))
            .Should().BeEquivalentTo([new FollowPair(Id(3), Id(4)), new FollowPair(Id(4), Id(3))]);
    }

    [Fact]
    public async Task Up_replaces_the_old_schema_with_the_follow_tables_and_their_unique_indexes()
    {
        await InsertOldDataAsync(Relationships, Requests);

        await MigrateToAsync(SplitMigration);

        (await TableExistsAsync("Follows")).Should().BeTrue();
        (await TableExistsAsync("FollowRelationships")).Should().BeFalse();
        (await ColumnExistsAsync("FollowRequests", "FollowRelationshipId")).Should().BeFalse();
        (await IndexDefinitionAsync("IX_Follows_FollowerId_FollowedUserId"))
            .Should().Contain("UNIQUE");
        foreach (var plainIndex in new[]
                 {
                     "IX_Follows_FollowedUserId", "IX_FollowRequests_SenderId", "IX_FollowRequests_RecipientId"
                 })
        {
            (await IndexDefinitionAsync(plainIndex)).Should().NotContain("UNIQUE", $"{plainIndex} must exist and be non-unique");
        }
        (await IndexDefinitionAsync("IX_FollowRequests_SenderId_RecipientId_Pending"))
            .Should().Contain("UNIQUE").And.Contain("WHERE").And.Contain("Pending");
    }

    [Fact]
    public async Task Down_restores_the_previous_schema_and_discards_all_follows_and_requests()
    {
        await InsertOldDataAsync(Relationships, Requests);
        await MigrateToAsync(SplitMigration);
        (await CountAsync("Follows")).Should().Be(ExpectedFollows.Length, "there must be something to lose");

        await MigrateToAsync(PreviousMigration());

        (await TableExistsAsync("Follows")).Should().BeFalse();
        (await TableExistsAsync("FollowRelationships")).Should().BeTrue();
        (await ColumnExistsAsync("FollowRequests", "FollowRelationshipId")).Should().BeTrue();
        (await CountAsync("FollowRelationships")).Should().Be(0);
        (await CountAsync("FollowRequests")).Should().Be(0, "Down cannot rebuild the old pairs, so it discards history");
    }

    [Fact]
    public async Task Down_leaves_a_schema_that_Up_can_migrate_again()
    {
        await InsertOldDataAsync(Relationships, Requests);
        await MigrateToAsync(SplitMigration);
        await MigrateToAsync(PreviousMigration());

        await InsertOldDataAsync(
            [new OldRelationship(Id(301), First: Id(21), Second: Id(22), FirstFollowsSecond: true, SecondFollowsFirst: false)],
            [new OldRequest(Id(401), Id(301), Sender: Id(21), Recipient: Id(22), FollowRequestStatus.Accepted, Created, Answered)]);
        await MigrateToAsync(SplitMigration);

        await using var context = CreateContext();
        (await context.Follows.AsNoTracking().ToListAsync())
            .Select(follow => new FollowPair(follow.FollowerId, follow.FollowedUserId))
            .Should().BeEquivalentTo([new FollowPair(Id(21), Id(22))]);
        (await context.FollowRequests.AsNoTracking().SingleAsync()).Id.Should().Be(Id(401));
    }

    [Fact]
    public async Task The_migrated_follow_tables_match_the_tables_created_directly_from_the_model()
    {
        // The migration was written by hand, and the snapshot check only compares the model with the snapshot.
        // This compares what the migration actually builds with what the model asks for.
        await MigrateToAsync(SplitMigration);
        var modelOnly = new NpgsqlConnectionStringBuilder(_connection.ConnectionString)
        {
            Database = _connection.Database + "-model"
        };
        await ExecuteOnServerAsync($"CREATE DATABASE \"{modelOnly.Database}\"");
        try
        {
            await using (var context = CreateContext(modelOnly.ConnectionString))
            {
                await context.Database.EnsureCreatedAsync();
            }

            var migrated = await DescribeFollowTablesAsync(_connection.ConnectionString);
            var created = await DescribeFollowTablesAsync(modelOnly.ConnectionString);

            migrated.Should().NotBeEmpty();
            migrated.Should().BeEquivalentTo(created);
        }
        finally
        {
            NpgsqlConnection.ClearAllPools();
            await ExecuteOnServerAsync($"DROP DATABASE IF EXISTS \"{modelOnly.Database}\" WITH (FORCE)");
        }
    }

    private static async Task<List<string>> DescribeFollowTablesAsync(string connectionString)
    {
        const string sql = """
            SELECT 'column ' || table_name || '.' || column_name || ' ' || data_type
                   || ' nullable=' || is_nullable
                   || ' length=' || COALESCE(character_maximum_length::text, '-')
                   || ' default=' || COALESCE(column_default, '-')
            FROM information_schema.columns
            WHERE table_schema = 'social' AND table_name IN ('Follows', 'FollowRequests')
            UNION ALL
            SELECT 'index ' || indexname || ' ' || indexdef
            FROM pg_indexes
            WHERE schemaname = 'social' AND tablename IN ('Follows', 'FollowRequests')
            UNION ALL
            SELECT 'constraint ' || conrelid::regclass::text || ' ' || conname || ' ' || pg_get_constraintdef(oid)
            FROM pg_constraint
            WHERE connamespace = 'social'::regnamespace
              AND conrelid::regclass::text IN ('social."Follows"', 'social."FollowRequests"')
            """;

        await using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        var description = new List<string>();
        while (await reader.ReadAsync())
        {
            description.Add(reader.GetString(0));
        }

        return description;
    }

    private static Guid Id(int number) => new($"00000000-0000-0000-0000-{number:D12}");

    private SocialDbContext CreateContext() => CreateContext(_connection.ConnectionString);

    private static SocialDbContext CreateContext(string connectionString) => new(
        new DbContextOptionsBuilder<SocialDbContext>()
            .UseNpgsql(connectionString,
                npgsql => npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "social"))
            .Options);

    private async Task MigrateToAsync(string targetMigration)
    {
        await using var context = CreateContext();
        await context.GetService<IMigrator>().MigrateAsync(targetMigration);
    }

    private string PreviousMigration()
    {
        using var context = CreateContext();
        var migrations = context.Database.GetMigrations().ToList();
        return migrations[migrations.IndexOf(SplitMigration) - 1];
    }

    private async Task InsertOldDataAsync(IEnumerable<OldRelationship> relationships, IEnumerable<OldRequest> requests)
    {
        await using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();

        foreach (var relationship in relationships)
        {
            await using var command = new NpgsqlCommand(
                """
                INSERT INTO social."FollowRelationships"
                    ("Id", "FirstUserId", "SecondUserId", "FirstFollowsSecond", "SecondFollowsFirst")
                VALUES (@id, @first, @second, @firstFollowsSecond, @secondFollowsFirst)
                """, connection);
            command.Parameters.AddWithValue("id", relationship.Id);
            command.Parameters.AddWithValue("first", relationship.First);
            command.Parameters.AddWithValue("second", relationship.Second);
            command.Parameters.AddWithValue("firstFollowsSecond", relationship.FirstFollowsSecond);
            command.Parameters.AddWithValue("secondFollowsFirst", relationship.SecondFollowsFirst);
            await command.ExecuteNonQueryAsync();
        }

        foreach (var request in requests)
        {
            await using var command = new NpgsqlCommand(
                """
                INSERT INTO social."FollowRequests"
                    ("Id", "SenderId", "RecipientId", "CreatedAt", "Status", "RespondedAt", "FollowRelationshipId")
                VALUES (@id, @sender, @recipient, @createdAt, @status, @respondedAt, @relationshipId)
                """, connection);
            command.Parameters.AddWithValue("id", request.Id);
            command.Parameters.AddWithValue("sender", request.Sender);
            command.Parameters.AddWithValue("recipient", request.Recipient);
            command.Parameters.Add(new NpgsqlParameter("createdAt", NpgsqlDbType.TimestampTz) { Value = request.CreatedAt });
            command.Parameters.AddWithValue("status", request.Status.ToString());
            command.Parameters.Add(new NpgsqlParameter("respondedAt", NpgsqlDbType.TimestampTz)
            {
                Value = (object?)request.RespondedAt ?? DBNull.Value
            });
            command.Parameters.AddWithValue("relationshipId", request.RelationshipId);
            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task<bool> TableExistsAsync(string table) =>
        await ScalarAsync<bool>($"SELECT to_regclass('social.\"{table}\"') IS NOT NULL");

    private async Task<bool> ColumnExistsAsync(string table, string column) =>
        await ScalarAsync<bool>(
            $"SELECT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_schema = 'social' " +
            $"AND table_name = '{table}' AND column_name = '{column}')");

    private async Task<string> IndexDefinitionAsync(string index) =>
        await ScalarAsync<string>($"SELECT indexdef FROM pg_indexes WHERE schemaname = 'social' AND indexname = '{index}'");

    private async Task<long> CountAsync(string table) =>
        await ScalarAsync<long>($"SELECT COUNT(*) FROM social.\"{table}\"");

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using var connection = new NpgsqlConnection(_connection.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync())!;
    }

    private async Task ExecuteOnServerAsync(string sql)
    {
        var server = new NpgsqlConnectionStringBuilder(_connection.ConnectionString) { Database = "postgres" };
        await using var connection = new NpgsqlConnection(server.ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync();
    }

    private sealed record OldRelationship(
        Guid Id, Guid First, Guid Second, bool FirstFollowsSecond, bool SecondFollowsFirst);

    private sealed record OldRequest(
        Guid Id, Guid RelationshipId, Guid Sender, Guid Recipient,
        FollowRequestStatus Status, DateTime CreatedAt, DateTime? RespondedAt);

    private sealed record FollowPair(Guid FollowerId, Guid FollowedUserId);

    private sealed record MigratedRequest(
        Guid Id, Guid SenderId, Guid RecipientId,
        FollowRequestStatus Status, DateTime CreatedAt, DateTime? RespondedAt);
}
