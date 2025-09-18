using Microsoft.Agents.DevUI.Services;
using Microsoft.Agents.DevUI.Models;
using Microsoft.Extensions.AI.Agents;
using Microsoft.Agents.Workflows;

namespace Microsoft.Agents.DevUI;

public class DevUIServer
{
    private readonly string? _entitiesDir;
    private readonly int _port;
    private readonly string _host;
    private readonly List<string> _corsOrigins;
    private readonly bool _uiEnabled;
    private readonly List<object> _inMemoryEntities = new();

    public DevUIServer(
        string? entitiesDir = null,
        int port = 8080,
        string host = "127.0.0.1",
        List<string>? corsOrigins = null,
        bool uiEnabled = true)
    {
        _entitiesDir = entitiesDir;
        _port = port;
        _host = host;
        _corsOrigins = corsOrigins ?? new List<string> { "*" };
        _uiEnabled = uiEnabled;
    }

    public void RegisterEntities(params object[] entities)
    {
        _inMemoryEntities.AddRange(entities);
    }

    public WebApplication CreateApp()
    {
        var builder = WebApplication.CreateBuilder();

        // Add services
        builder.Services.AddControllers();
        // TODO: Add Swagger when available in central packages
        // builder.Services.AddEndpointsApiExplorer();
        // builder.Services.AddSwaggerGen();

        // Add CORS
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (_corsOrigins.Contains("*"))
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                }
                else
                {
                    policy.WithOrigins(_corsOrigins.ToArray())
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                }
            });
        });

        // Add our services
        builder.Services.AddSingleton<EntityDiscoveryService>();
        builder.Services.AddSingleton<ExecutionService>();
        builder.Services.AddSingleton<ThreadService>();

        var app = builder.Build();

        // Configure pipeline
        // TODO: Add Swagger when available
        // if (app.Environment.IsDevelopment())
        // {
        //     app.UseSwagger();
        //     app.UseSwaggerUI();
        // }

        app.UseCors();
        app.UseRouting();
        app.MapControllers();

        // Initialize entity discovery
        var discoveryService = app.Services.GetRequiredService<EntityDiscoveryService>();

        // Discover entities from directory
        if (!string.IsNullOrEmpty(_entitiesDir))
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    await discoveryService.DiscoverEntitiesFromDirectoryAsync(_entitiesDir);
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<DevUIServer>>();
                    logger.LogError(ex, "Failed to discover entities from directory");
                }
            });
        }

        // Register in-memory entities
        foreach (var entity in _inMemoryEntities)
        {
            discoveryService.RegisterInMemoryEntity(entity);
        }

        // Serve UI if enabled
        if (_uiEnabled)
        {
            // In a real implementation, you'd serve the React UI here
            // For now, just add a simple info endpoint
            app.MapGet("/", () => new
            {
                message = "Agent Framework DevUI Server",
                endpoints = new
                {
                    health = "/health",
                    entities = "/v1/entities",
                    responses = "/v1/responses"
                }
            });
        }

        return app;
    }

    public async Task RunAsync()
    {
        var app = CreateApp();

        var logger = app.Services.GetRequiredService<ILogger<DevUIServer>>();
        logger.LogInformation("Starting Agent Framework DevUI on {Host}:{Port}", _host, _port);

        await app.RunAsync($"http://{_host}:{_port}");
    }
}