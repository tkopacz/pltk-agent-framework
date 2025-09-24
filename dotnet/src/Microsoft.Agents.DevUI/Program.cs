using System.CommandLine;
using Microsoft.Agents.DevUI;

// CLI Command setup
var entitiesDirOption = new Option<string?>(
    "--entities-dir",
    description: "Directory to scan for agent/workflow entities");

var portOption = new Option<int>(
    "--port",
    getDefaultValue: () => 8080,
    description: "Port to run the server on");

var hostOption = new Option<string>(
    "--host",
    getDefaultValue: () => "127.0.0.1",
    description: "Host to bind the server to");

var autoOpenOption = new Option<bool>(
    "--auto-open",
    getDefaultValue: () => false,
    description: "Automatically open browser when server starts");

var rootCommand = new RootCommand("Agent Framework DevUI - Development server for .NET agents and workflows")
{
    entitiesDirOption,
    portOption,
    hostOption,
    autoOpenOption
};

rootCommand.SetHandler(async (entitiesDir, port, host, autoOpen) =>
{
    Console.WriteLine("🚀 Starting Agent Framework DevUI for .NET");
    Console.WriteLine($"📁 Entities directory: {entitiesDir ?? "none (in-memory only)"}");
    Console.WriteLine($"🌐 Server: http://{host}:{port}");
    Console.WriteLine();

    try
    {
        // Check if we should load sample entities
        object[]? entities = null;
        if (entitiesDir?.Contains("samples") == true)
        {
            Console.WriteLine("📦 Loading built-in sample entities...");
            var weatherAgent = new Microsoft.Agents.DevUI.Samples.WeatherAgent();
            var simpleWorkflow = Microsoft.Agents.DevUI.Samples.SimpleWorkflow.Create();
            entities = new object[] { weatherAgent, simpleWorkflow };
        }

        await DevUI.ServeAsync(
            entities: entities,
            entitiesDir: entitiesDir,
            port: port,
            host: host,
            autoOpen: autoOpen);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error starting server: {ex.Message}");
        Environment.Exit(1);
    }
}, entitiesDirOption, portOption, hostOption, autoOpenOption);

// Add examples command
var examplesCommand = new Command("examples", "Show usage examples");
examplesCommand.SetHandler(() =>
{
    Console.WriteLine("Agent Framework DevUI Examples:");
    Console.WriteLine();
    Console.WriteLine("1. Start server with entities from directory:");
    Console.WriteLine("   dotnet run -- --entities-dir ./samples --port 8080");
    Console.WriteLine();
    Console.WriteLine("2. Start server for in-memory entities only:");
    Console.WriteLine("   dotnet run -- --port 8080");
    Console.WriteLine();
    Console.WriteLine("3. Start server with auto-open browser:");
    Console.WriteLine("   dotnet run -- --entities-dir ./samples --auto-open");
    Console.WriteLine();
    Console.WriteLine("4. Custom host and port:");
    Console.WriteLine("   dotnet run -- --host 0.0.0.0 --port 3000");
    Console.WriteLine();
    Console.WriteLine("The server provides OpenAI-compatible API endpoints:");
    Console.WriteLine("  • GET  /health              - Health check");
    Console.WriteLine("  • GET  /v1/entities         - List all entities");
    Console.WriteLine("  • GET  /v1/entities/{id}/info - Get entity details");
    Console.WriteLine("  • POST /v1/responses        - Execute entity (streaming/non-streaming)");
});

rootCommand.AddCommand(examplesCommand);

return await rootCommand.InvokeAsync(args);