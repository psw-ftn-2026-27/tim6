using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Infrastructure;
using Social.Application;
using Social.Application.BlogAuthoring;
using Social.Application.BlogCommenting;
using Social.Application.BlogReading;
using Social.Application.BlogVoting;
using Social.Application.Blogs;
using Social.Application.Following;
using Social.Application.ClubAuthoring;
using Social.Application.ClubMembership;
using Social.Application.ClubModeration;
using Social.Application.ClubReading;
using Social.Application.Clubs;
using Social.Domain.Following;
using Social.Infrastructure.Persistence;
using Social.Infrastructure.Persistence.Blogs;
using Social.Infrastructure.Persistence.Following;
using Social.Infrastructure.Persistence.Clubs;
using Social.Application.Conversations;
using Social.Application.DirectConversationMessaging;
using Social.Infrastructure.Persistence.Conversations;

namespace Social.Infrastructure;

public static class SocialModuleExtensions
{
    public static IServiceCollection AddSocialModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddModuleDbContext<SocialDbContext>(configuration, "social");
        services.AddHostedService<SocialModuleInitializer>();
        services.AddAutoMapper(mapper => mapper.AddProfile<SocialMapperProfile>());

        services.AddScoped<IBlogRepository, BlogRepository>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<SocialDbContext>());
        services.AddScoped<IBlogReadRepository, BlogReadRepository>();
        services.AddScoped<IFollowRepository, FollowRepository>();
        services.AddScoped<IFollowRequestRepository, FollowRequestRepository>();
        services.AddScoped<IFollowReadRepository, FollowReadRepository>();
        services.AddScoped<IClubRepository, ClubRepository>();
        services.AddScoped<IClubReadRepository, ClubReadRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IChatMessageRepository, ChatMessageRepository>();
        services.AddScoped<IConversationReadRepository, ConversationReadRepository>();

        services.AddScoped<BlogAuthoringService>();
        services.AddScoped<BlogAuthoringQueries>();
        services.AddScoped<BlogReadingQueries>();
        services.AddScoped<BlogCommentingService>();
        services.AddScoped<BlogVotingService>();
        services.AddScoped<BlogVotingQueries>();
        services.AddScoped<FollowRequestProcessor>();
        services.AddScoped<FollowingService>();
        services.AddScoped<FollowingQueries>();
        services.AddScoped<ClubAuthoringService>();
        services.AddScoped<ClubAuthoringQueries>();
        services.AddScoped<ClubReadingQueries>();
        services.AddScoped<ClubMembershipService>();
        services.AddScoped<ClubMembershipQueries>();
        services.AddScoped<ClubModerationService>();
        services.AddScoped<ClubModerationQueries>();
        services.AddScoped<DirectConversationMessagingService>();
        services.AddScoped<DirectConversationMessagingQueries>();

        return services;
    }
}
