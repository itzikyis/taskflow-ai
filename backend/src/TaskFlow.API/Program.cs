using TaskFlow.Application;
using TaskFlow.Infrastructure;
using TaskFlow.API.Middleware;

// Npgsql 6+ requires UTC DateTimes for timestamptz columns; this switch restores
// the pre-6 behavior so existing DateTime fields (Kind=Unspecified) are accepted.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Serialize/deserialize enums as their string names (e.g. "Medium", "Todo")
        // so the frontend can use human-readable values without manual int mapping.
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "TaskFlow API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable request body buffering so the Slack command endpoint can re-read the
// raw body when computing the HMAC-SHA256 signing-secret verification.
app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/api/slack/command"))
        context.Request.EnableBuffering();
    await next();
});

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program discoverable for WebApplicationFactory in integration tests
public partial class Program { }
