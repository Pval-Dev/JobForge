using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using JobForge.Persistence;
using JobForge.Job;
using JobForge.Users;
using JobForge.Audit;
using JobForge.Queues;
using JobForge.Schedule;
using JobForge.Webhooks;
using JobForge.Retry;
using JobForge.Workers;
using JobForge.Metrics;
using JobForge.EndPoints;
using JobForge.Common;
using JobForge.Auth;


var builder = WebApplication.CreateBuilder(args);


// =======================
// INFRASTRUCTURE
// =======================

builder.Services.AddOpenApi();

builder.Services.AddDbContext<JobForgeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString(
            "JobForgeDatabase"
        )
    )
);


// =======================
// REPOSITORIES
// =======================

builder.Services.AddScoped<
    IJobRepository,
    JobRepository
>();

builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuditRepository>();
builder.Services.AddScoped<ScheduleRepository>();
builder.Services.AddScoped<RetryAttemptRepository>();
builder.Services.AddScoped<WebhookRepository>();


// =======================
// APPLICATION SERVICES
// =======================

builder.Services.AddScoped<AuditService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<JobService>();
builder.Services.AddScoped<QueueService>();
builder.Services.AddScoped<SchedulerService>();
builder.Services.AddScoped<RetryService>();
builder.Services.AddScoped<WebhookService>();
builder.Services.AddScoped<MetricsService>();


// =======================
// PASSWORDS / AUTH
// =======================

builder.Services.AddScoped<
    IPasswordHasher<User>,
    PasswordHasher<User>
>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<InitialAdminBootstrapper>();


// =======================
// GLOBAL EXCEPTIONS
// =======================

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


// =======================
// AUTHENTICATION
// =======================

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme
    )
    .AddJwtBearer(options =>
    {
        string jwtKey =
            builder.Configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured."
            );

        string issuer =
            builder.Configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured."
            );

        string audience =
            builder.Configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured."
            );

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)
                    ),
                NameClaimType = ClaimTypes.Name,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
    });


// =======================
// AUTHORIZATION
// =======================

builder.Services.AddAuthorization(options =>
{
    // Every endpoint requires authentication by default
    // unless it explicitly opts out with .AllowAnonymous().
    options.FallbackPolicy =
        new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

    options.AddPolicy(
        "AdminOnly",
        policy =>
            policy.RequireRole(
                UserRole.Admin.ToString()
            )
    );
});


// =======================
// JOB RUNTIME
// =======================

builder.Services.AddScoped<JobClaimService>();
builder.Services.AddScoped<JobProcessor>();
builder.Services.AddScoped<JobExecutor>();
builder.Services.AddScoped<JobRecoveryService>();

builder.Services.AddSingleton<RetryPolicy>();
builder.Services.AddSingleton<WorkerRegistry>();


// =======================
// JOB HANDLERS
// =======================

builder.Services.AddJobForgeHandlers();


// =======================
// WEBHOOK HTTP CLIENT
// =======================

builder.Services.AddHttpClient(
    "Webhooks",
    client =>
    {
        client.Timeout = TimeSpan.FromSeconds(10);
    }
);


// =======================
// BACKGROUND SERVICES
// =======================

builder.Services.AddHostedService<WorkerBackgroundService>();
builder.Services.AddHostedService<SchedulerBackgroundService>();


// =======================
// BUILD
// =======================

var app = builder.Build();


// =======================
// STARTUP
// =======================

using (var scope = app.Services.CreateScope())
{
    var bootstrapper =
        scope.ServiceProvider
            .GetRequiredService<InitialAdminBootstrapper>();

    bootstrapper.EnsureAdminExists();

    var recoveryService =
        scope.ServiceProvider
            .GetRequiredService<JobRecoveryService>();

    int recoveredJobs =
        recoveryService.RecoverInterruptedJobs();

    if (recoveredJobs > 0)
    {
        Console.WriteLine(
            $"[RECOVERY] Recovered {recoveredJobs} interrupted job(s)."
        );
    }
}


// =======================
// DEVELOPMENT
// =======================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi()
        .AllowAnonymous();
}


// =======================
// MIDDLEWARE
// =======================

app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();


// =======================
// HTTP API
// =======================

app.MapAuthEndPoints();
app.MapUserEndPoints();
app.MapJobEndPoints();
app.MapScheduleEndPoints();
app.MapWebhookEndPoints();
app.MapMetricsEndPoints();
app.MapAuditEndPoints();
app.MapWorkerEndPoints();


// =======================
// RUN
// =======================

app.Run();
