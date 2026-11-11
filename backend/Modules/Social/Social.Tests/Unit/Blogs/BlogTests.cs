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
    public void User_edits_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.UpdateComment(comment.Id, WellKnownUsers.Administrator, "Ipak prosečna tura.");

        blog.Comments.Single().Text.Should().Be("Ipak prosečna tura.");
    }

    [Fact]
    public void User_cannot_edit_another_users_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        var update = () => blog.UpdateComment(comment.Id, WellKnownUsers.Explorer, "Izmena tuđeg komentara.");

        update.Should().Throw<NotFoundException>();
    }

    [Fact]
    public void User_deletes_their_own_comment()
    {
        var blog = CreatePublishedBlog();
        blog.AddComment(WellKnownUsers.Administrator, "Odlična tura!");
        var comment = blog.Comments.Single();

        blog.DeleteComment(comment.Id, WellKnownUsers.Administrator);

        blog.Comments.Should().BeEmpty();
    }

}
