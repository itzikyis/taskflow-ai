using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Slack;
using TaskFlow.Infrastructure.Persistence;
using TaskFlow.Infrastructure.Persistence.Repositories;
using TaskFlow.Infrastructure.Services;

namespace TaskFlow.Infrastructure;

/// <summary>Registers Infrastructure-layer services with the DI container.</summary>
public static class DependencyInjection
{
    /// <summary>Adds Infrastructure services (EF Core, repositories).</summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITaskRepository, TaskRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICommentRepository, CommentRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<ITeamRepository, TeamRepository>();
        services.AddScoped<IActivityLogRepository, ActivityLogRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IDevelopmentLinkRepository, DevelopmentLinkRepository>();
        services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
        services.AddScoped<ITaskDependencyRepository, TaskDependencyRepository>();
        services.AddScoped<IAutomationRuleRepository, AutomationRuleRepository>();
        services.AddScoped<IAutomationEvaluatorService, AutomationEvaluatorService>();
        services.AddScoped<IInitiativeRepository, InitiativeRepository>();
        services.AddScoped<IProjectDocumentRepository, ProjectDocumentRepository>();
        services.AddScoped<ICustomFieldRepository, CustomFieldRepository>();
        services.AddSingleton<IGitHubWebhookParser, GitHubWebhookParser>();
        services.AddSingleton<ICalendarFeedBuilder, RfcCalendarFeedBuilder>();
        services.AddScoped<ISlackIntegrationRepository, SlackIntegrationRepository>();
        services.AddSingleton<ISlackOptions, SlackOptions>();
        services.AddHttpClient<IExternalNotificationService, SlackNotificationService>();
        services.AddSingleton<ITaskSearchInterpreter, KeywordTaskSearchInterpreter>();
        services.AddSingleton<IDuplicateTaskDetectionService, TextSimilarityDuplicateDetectionService>();
        services.AddHttpClient<IAiAssistantService, ClaudeAiAssistantService>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();

        // JWT authentication
        var jwt = configuration.GetSection("Jwt");
        var secretKey = jwt["SecretKey"]
            ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer              = jwt["Issuer"]   ?? "taskflow-api",
                    ValidAudience            = jwt["Audience"] ?? "taskflow-client",
                    IssuerSigningKey         = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey)),
                };
            });

        return services;
    }
}
