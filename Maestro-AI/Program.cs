using Maestro_AI;
using Maestro_AI.Api;
using Maestro_AI.Components;
using Maestro_AI.Hardware;
using Maestro_AI.Models;
using Maestro_AI.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Used to get httpContext in razor pages
builder.Services.AddHttpContextAccessor();

// CORS: allow the static HTML client (served separately) to call the API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// Enable verbose testing log (set to true for debugging)
Log.IsEnabled = false;
Log.LogStep("=== SERVER START ===");

// Initialize hardware driver from appsettings.json
var hwConfig = app.Configuration.GetSection("Hardware").Get<HardwareConfig>() ?? new HardwareConfig();
HardwareManager.Instance.Initialize(hwConfig);

// Initialize AI feature flags from appsettings.json
var aiConfig = app.Configuration.GetSection("AiFeatures").Get<AiFeaturesConfig>() ?? new AiFeaturesConfig();
FeatureFlags.Init(aiConfig);

// Initialize external instruments from appsettings.json
var instrConfig = app.Configuration.GetSection("Instruments").Get<InstrumentsConfig>() ?? new InstrumentsConfig();
InstrumentManager.Init(instrConfig);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseCors();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

// ── API endpoint (UISupportBlazor middleware) ─────────────────────────
// MasterAPI routes all calls through a single /api endpoint.
// GET  /api        → returns generated C# client code for all methods
// POST /api/{method} → invokes the method with JSON body

// API logging middleware — logs every POST to /api/*
app.Use(async (context, next) =>
{
    if (context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api"))
    {
        var path = context.Request.Path.ToString();
        // Read body into memory (Kestrel stream doesn't support Position)
        using var bodyReader = new System.IO.StreamReader(context.Request.Body);
        var body = await bodyReader.ReadToEndAsync();
        var bodyBytes = System.Text.Encoding.UTF8.GetBytes(body ?? "");
        context.Request.Body = new MemoryStream(bodyBytes);

        Log.LogStep("[API] " + path + " body=" + (body?.Length > 120 ? body[..120] + "..." : body));

        // Capture response
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await next();

        memStream.Position = 0;
        var response = await new System.IO.StreamReader(memStream).ReadToEndAsync();
        memStream.Position = 0;
        await memStream.CopyToAsync(originalBody);
        context.Response.Body = originalBody;

        Log.LogStep("[API] " + path + " => " + (response?.Length > 200 ? response[..200] + "..." : response));
    }
    else
    {
        await next();
    }
});

app.UseMiddleware<UISupportBlazor.ApiMiddleware>(typeof(MasterAPI), "");

app.Run();
