using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Shared.Tests;
using Social.Application.BlogAuthoring;
using Social.Application.Blogs;
using Social.Domain.Blogs;
using Social.Infrastructure.Persistence;
using Social.Tests.Integration.Seeds;
using Xunit;

namespace Social.Tests.Integration.BlogAuthoring;

public class BlogAuthoringCommandTests : BaseIntegrationTest
{
    public BlogAuthoringCommandTests(SocialApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_creates_a_draft_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", ["https://example.com/slika.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await response.Content.ReadFromJsonAsync<BlogDto>(JsonOptions);
        created!.AuthorId.Should().Be(WellKnownUsers.Explorer);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore + 1);
        var stored = assertContext.Blogs.Single(blog => blog.Id == created.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
        stored.Images.Should().ContainSingle().Which.Should().Be("https://example.com/slika.jpg");
        stored.Comments.Should().BeEmpty();
    }

    [Fact]
    public async Task Explorer_cannot_create_a_blog_without_a_title()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CreateBlogDto("   ", "Opis novog bloga.", ["https://example.com/slika.jpg"]);
        using var arrangeContext = Factory.CreateContext<SocialDbContext>();
        var blogCountBefore = arrangeContext.Blogs.Count();

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        assertContext.Blogs.Count().Should().Be(blogCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_blog()
    {
        var client = Factory.CreateClient();
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", ["https://example.com/slika.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_create_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CreateBlogDto("Novi blog", "Opis novog bloga.", ["https://example.com/slika.jpg"]);

        var response = await client.PostAsJsonAsync("/api/social/blogs", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Draft_blog_is_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Published);
    }

    [Fact]
    public async Task Explorer_cannot_publish_another_authors_blog()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.PostAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Status.Should().Be(BlogStatus.Draft);
    }

    [Fact]
    public async Task Author_updates_their_blog_with_a_new_modification_date()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateBlogDto("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be("Novi utisci sa tvrđave");
        stored.Description.Should().Be("Dopunjen opis obilaska.");
        stored.Images.Should().Equal("https://example.com/kula.jpg");
        stored.LastModifiedAt.Should().BeAfter(BlogSeed.FortressImpressions.LastModifiedAt);
    }

    [Fact]
    public async Task Author_cannot_update_another_authors_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateBlogDto("Izmena tuđeg bloga", "Izmenjen opis.", ["https://example.com/kula.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.SecondExplorersDiary.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.SecondExplorersDiary.Id);
        stored.Title.Should().Be(BlogSeed.SecondExplorersDiary.Title);
        stored.Description.Should().Be(BlogSeed.SecondExplorersDiary.Description);
        stored.Images.Should().Equal(BlogSeed.SecondExplorersDiary.Images);
    }

    [Fact]
    public async Task Update_without_an_image_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new UpdateBlogDto("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", []);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<SocialDbContext>();
        var stored = assertContext.Blogs.Single(blog => blog.Id == BlogSeed.FortressImpressions.Id);
        stored.Title.Should().Be(BlogSeed.FortressImpressions.Title);
        stored.Description.Should().Be(BlogSeed.FortressImpressions.Description);
        stored.Images.Should().Equal(BlogSeed.FortressImpressions.Images);
    }

    [Fact]
    public async Task Anonymous_user_cannot_update_a_blog()
    {
        var client = Factory.CreateClient();
        var request = new UpdateBlogDto("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_update_a_blog()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new UpdateBlogDto("Novi utisci sa tvrđave", "Dopunjen opis obilaska.", ["https://example.com/kula.jpg"]);

        var response = await client.PutAsJsonAsync($"/api/social/blogs/{BlogSeed.FortressImpressions.Id}", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
