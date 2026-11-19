using System.Text;
using System.Text.Json.Serialization;
using Exploration.Api;
using Exploration.Infrastructure;
using Games.Api;
using Games.Infrastructure;
using Host.Api;
using Identity.Api;
using Identity.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Payment.Api;
using Payment.Infrastructure;
using Scalar.AspNetCore;
using Shared.Api;
using Shared.Infrastructure;
using Social.Api;
using Social.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddIdentityControllers()
    .AddExplorationControllers()
    .AddGamesControllers()
    .AddSocialControllers()
    .AddPaymentControllers()
    .AddSharedControllers();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("Jwt");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!))
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                if (accessToken.Count > 0 && context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services
    .AddIdentityModule(builder.Configuration)
    .AddExplorationModule(builder.Configuration)
    .AddGamesModule(builder.Configuration)
    .AddSocialModule(builder.Configuration)
    .AddPaymentModule(builder.Configuration)
    .AddShared(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.AddPreferredSecuritySchemes("Bearer"));
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapSocialConversationHub();
app.MapExplorationConversationHub();

app.Run();

public partial class Program;
