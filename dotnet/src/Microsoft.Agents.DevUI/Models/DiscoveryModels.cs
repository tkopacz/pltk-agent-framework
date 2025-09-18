using System.Text.Json.Serialization;

namespace Microsoft.Agents.DevUI.Models;

public class EntityInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("type")]
    public string Type { get; set; } = "";

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("input_schema")]
    public Dictionary<string, object>? InputSchema { get; set; }

    [JsonPropertyName("input_type_name")]
    public string? InputTypeName { get; set; }

    [JsonPropertyName("workflow_dump")]
    public object? WorkflowDump { get; set; }
}

public class DiscoveryResponse
{
    [JsonPropertyName("entities")]
    public List<EntityInfo> Entities { get; set; } = new();
}