using Microsoft.Agents.DevUI.Models;
using Microsoft.Extensions.AI.Agents;
using Microsoft.Agents.Workflows;
using System.Reflection;

namespace Microsoft.Agents.DevUI.Services;

public class EntityDiscoveryService
{
    private readonly Dictionary<string, EntityInfo> _entityInfos = new();
    private readonly Dictionary<string, object> _entityObjects = new();
    private readonly ILogger<EntityDiscoveryService> _logger;

    public EntityDiscoveryService(ILogger<EntityDiscoveryService> logger)
    {
        _logger = logger;
    }

    public async Task<List<EntityInfo>> DiscoverEntitiesFromDirectoryAsync(string? entitiesDir)
    {
        var entities = new List<EntityInfo>();

        if (string.IsNullOrEmpty(entitiesDir) || !Directory.Exists(entitiesDir))
        {
            _logger.LogWarning("Entities directory not found or not specified: {Dir}", entitiesDir);
            return entities;
        }

        _logger.LogInformation("Discovering entities from directory: {Dir}", entitiesDir);

        // Scan for .cs files and try to load types
        var csFiles = Directory.GetFiles(entitiesDir, "*.cs", SearchOption.AllDirectories);

        foreach (var file in csFiles)
        {
            try
            {
                // This is a simplified discovery - in a real implementation,
                // you might want to compile and load the assemblies dynamically
                var content = await File.ReadAllTextAsync(file);

                // Look for agent or workflow patterns
                if (content.Contains(": AIAgent") || content.Contains("public class") && content.Contains("Agent"))
                {
                    var entityId = $"agent_{Path.GetFileNameWithoutExtension(file).ToLowerInvariant()}";
                    var entityInfo = new EntityInfo
                    {
                        Id = entityId,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Type = "agent",
                        Description = $"Agent from {Path.GetFileName(file)}"
                    };

                    entities.Add(entityInfo);
                    _entityInfos[entityId] = entityInfo;
                    _logger.LogInformation("Discovered agent: {Id}", entityId);
                }

                if (content.Contains(": Workflow") || content.Contains("public class") && content.Contains("Workflow"))
                {
                    var entityId = $"workflow_{Path.GetFileNameWithoutExtension(file).ToLowerInvariant()}";
                    var entityInfo = new EntityInfo
                    {
                        Id = entityId,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Type = "workflow",
                        Description = $"Workflow from {Path.GetFileName(file)}",
                        InputSchema = new Dictionary<string, object> { { "type", "string" } },
                        InputTypeName = "String"
                    };

                    entities.Add(entityInfo);
                    _entityInfos[entityId] = entityInfo;
                    _logger.LogInformation("Discovered workflow: {Id}", entityId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to process file: {File}", file);
            }
        }

        _logger.LogInformation("Discovered {Count} entities total", entities.Count);
        return entities;
    }

    public void RegisterInMemoryEntity(object entity)
    {
        var entityInfo = CreateEntityInfoFromObject(entity);
        _entityInfos[entityInfo.Id] = entityInfo;
        _entityObjects[entityInfo.Id] = entity;
        _logger.LogInformation("Registered in-memory entity: {Id}", entityInfo.Id);
    }

    private EntityInfo CreateEntityInfoFromObject(object entity)
    {
        var type = entity.GetType();
        var entityId = $"{type.Name.ToLowerInvariant()}_{Guid.NewGuid().ToString("N")[..8]}";

        var entityInfo = new EntityInfo
        {
            Id = entityId,
            Name = type.Name,
            Type = DetermineEntityType(entity),
            Description = $"In-memory {type.Name}"
        };

        // Additional setup for specific types
        if (entity is AIAgent agent)
        {
            entityInfo.Name = agent.Name ?? agent.Id;
            entityInfo.Description = agent.Description ?? entityInfo.Description;
        }

        return entityInfo;
    }

    private static string DetermineEntityType(object entity)
    {
        return entity switch
        {
            AIAgent => "agent",
            Workflow => "workflow",
            _ => "unknown"
        };
    }

    public List<EntityInfo> ListEntities()
    {
        return _entityInfos.Values.ToList();
    }

    public EntityInfo? GetEntityInfo(string entityId)
    {
        return _entityInfos.TryGetValue(entityId, out var info) ? info : null;
    }

    public object? GetEntityObject(string entityId)
    {
        return _entityObjects.TryGetValue(entityId, out var obj) ? obj : null;
    }
}