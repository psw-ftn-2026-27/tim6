using FluentAssertions;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Social.Domain.Blogs;
using Xunit;

namespace Social.Tests.Unit.Blogs;

public class BlogTests
{
    private static Blog CreateBlog() =>
        new(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

    private static Blog CreatePublishedBlog()
    {
        var blog = CreateBlog();
        blog.Publish();
        return blog;
    }

    private static Blog CreateArchivedBlog()
    {
        var blog = CreatePublishedBlog();
        blog.Archive();
        return blog;
    }

    private static Blog CreateBlogIn(BlogStatus status) =>
        status == BlogStatus.Draft ? CreateBlog() : CreateArchivedBlog();

    private static List<string> CreateImages(int count) =>
        Enumerable.Range(1, count).Select(number => $"https://example.com/slika-{number}.jpg").ToList();

    [Fact]
    public void New_blog_starts_as_a_draft()
    {
        var blog = CreateBlog();

        blog.Status.Should().Be(BlogStatus.Draft);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Blog_cannot_be_created_without_any_image()
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", "Opis obilaska tvrđave.", []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_title(string title)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, title, "Opis obilaska tvrđave.", ["https://example.com/tvrdjava.jpg"]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void New_blog_requires_a_description(string description)
    {
        var creation = () => new Blog(WellKnownUsers.Explorer, "Utisci sa tvrđave", description, ["https://example.com/tvrdjava.jpg"]);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Author_updates_the_title_description_and_images_of_a_blog()
    {
        var blog = CreatePublishedBlog();
        var createdAt = blog.CreatedAt;
        var updateStartedAt = DateTime.UtcNow;

        blog.Update("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        blog.Title.Should().Be("Novi utisci sa tvrđave");
        blog.Description.Should().Be("Dopunjen opis obilaska.");
        blog.Images.Should().Equal("https://example.com/kula.jpg");
        blog.LastModifiedAt.Should().BeOnOrAfter(updateStartedAt);
        blog.CreatedAt.Should().Be(createdAt);
        blog.AuthorId.Should().Be(WellKnownUsers.Explorer);
        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Theory]
    [InlineData("", "Dopunjen opis obilaska.", 1)]
    [InlineData("   ", "Dopunjen opis obilaska.", 1)]
    [InlineData("Novi utisci sa tvrđave", "", 1)]
    [InlineData("Novi utisci sa tvrđave", "   ", 1)]
    [InlineData("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", 0)]
    [InlineData("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", 11)]
    public void Update_without_required_content_or_with_too_many_images_is_rejected(string title, string description, int imageCount)
    {
        var blog = CreateBlog();

        var update = () => blog.Update(title, description, CreateImages(imageCount));

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_blog_is_published()
    {
        var blog = CreateBlog();

        blog.Publish();

        blog.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public void Published_blog_cannot_be_published_again()
    {
        var blog = CreatePublishedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_comments_on_a_published_blog()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        blog.Comments.Should().ContainSingle(comment =>
            comment.UserId == WellKnownUsers.Administrator && comment.Text == "Odlična tura!");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Comment_requires_text(string text)
    {
        var blog = CreatePublishedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, text);

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void New_comment_was_last_modified_when_it_was_created()
    {
        var blog = CreatePublishedBlog();

        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        var comment = blog.Comments.Single();
        comment.LastModifiedAt.Should().Be(comment.CreatedAt);
    }

    [Fact]
    public void User_edits_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", comment.CreatedAt.AddMinutes(1));

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void Edit_in_the_fourteenth_minute_changes_the_text_and_modification_time_but_not_the_creation_time()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();
        var createdAt = comment.CreatedAt;
        var now = createdAt.AddMinutes(14);

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", now);

        comment.Text.Should().Be("Ipak prosečna tura.");
        comment.LastModifiedAt.Should().Be(now);
        comment.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Comment_edited_in_the_fourteenth_minute_cannot_be_edited_exactly_fifteen_minutes_after_posting()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();
        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.", comment.CreatedAt.AddMinutes(14));

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak loša tura.", comment.CreatedAt.AddMinutes(15));

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void User_cannot_edit_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.", comment.CreatedAt.AddMinutes(1));

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_deletes_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator, comment.CreatedAt.AddMinutes(1));

        blog.Comments.Should().BeEmpty();
    }

    [Fact]
    public void Comment_cannot_be_deleted_exactly_fifteen_minutes_after_posting_and_stays_on_the_blog()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var deletion = () => blog.DeleteComment(comment.Id, WellKnownUsers.Administrator, comment.CreatedAt.AddMinutes(15));

        deletion.Should().Throw<DomainException>();
        blog.Comments.Should().ContainSingle(remaining => remaining.Id == comment.Id);
    }

    [Fact]
    public void Published_blog_is_archived_without_changing_its_modification_date()
    {
        var blog = CreatePublishedBlog();
        var lastModifiedAt = blog.LastModifiedAt;

        blog.Archive();

        blog.Status.Should().Be(BlogStatus.Closed);
        blog.LastModifiedAt.Should().Be(lastModifiedAt);
    }

    [Fact]
    public void Draft_blog_cannot_be_archived()
    {
        var blog = CreateBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_archived_again()
    {
        var blog = CreateArchivedBlog();

        var archiving = () => blog.Archive();

        archiving.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_updated()
    {
        var blog = CreateArchivedBlog();

        var update = () => blog.Update("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        update.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_be_published_again()
    {
        var blog = CreateArchivedBlog();

        var publishing = () => blog.Publish();

        publishing.Should().Throw<DomainException>();
    }

    [Fact]
    public void Archived_blog_cannot_receive_a_comment()
    {
        var blog = CreateArchivedBlog();

        var addition = () => blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");

        addition.Should().Throw<DomainException>();
    }

    [Fact]
    public void First_vote_of_a_user_is_recorded_with_its_sign_and_time()
    {
        var blog = CreatePublishedBlog();
        var now = DateTime.UtcNow;

        blog.CastVote(WellKnownUsers.Administrator, VoteSign.Up, now);

        var vote = blog.Votes.Should().ContainSingle().Subject;
        vote.UserId.Should().Be(WellKnownUsers.Administrator);
        vote.Sign.Should().Be(VoteSign.Up);
        vote.VotedAt.Should().Be(now);
    }

    [Theory]
    [InlineData(VoteSign.Up, VoteSign.Down)]
    [InlineData(VoteSign.Down, VoteSign.Up)]
    public void Opposite_vote_changes_the_existing_vote_and_refreshes_its_time(VoteSign first, VoteSign second)
    {
        var blog = CreatePublishedBlog();
        var votedAt = DateTime.UtcNow;
        blog.CastVote(WellKnownUsers.Administrator, first, votedAt);
        var changedAt = votedAt.AddMinutes(5);

        blog.CastVote(WellKnownUsers.Administrator, second, changedAt);

        var vote = blog.Votes.Should().ContainSingle().Subject;
        vote.UserId.Should().Be(WellKnownUsers.Administrator);
        vote.Sign.Should().Be(second);
        vote.VotedAt.Should().Be(changedAt);
    }

    [Theory]
    [InlineData(VoteSign.Up)]
    [InlineData(VoteSign.Down)]
    public void Same_vote_withdraws_the_existing_vote(VoteSign sign)
    {
        var blog = CreatePublishedBlog();
        var votedAt = DateTime.UtcNow;
        blog.CastVote(WellKnownUsers.Administrator, sign, votedAt);

        blog.CastVote(WellKnownUsers.Administrator, sign, votedAt.AddMinutes(5));

        blog.Votes.Should().BeEmpty();
    }

    [Theory]
    [InlineData(BlogStatus.Draft)]
    [InlineData(BlogStatus.Closed)]
    public void Vote_on_a_blog_that_is_not_published_is_rejected(BlogStatus status)
    {
        var blog = CreateBlogIn(status);

        var voting = () => blog.CastVote(WellKnownUsers.Administrator, VoteSign.Up, DateTime.UtcNow);

        voting.Should().Throw<DomainException>();
        blog.Votes.Should().BeEmpty();
    }
}
