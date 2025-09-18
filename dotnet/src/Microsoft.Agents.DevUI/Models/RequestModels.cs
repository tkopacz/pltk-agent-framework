using System.Text.Json.Serialization;

namespace Microsoft.Agents.DevUI.Models;

public class DevUIExecutionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = "agent-framework";

    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;

    [JsonPropertyName("messages")]
    public List<DevUIRequestMessage>? Messages { get; set; }

    [JsonPropertyName("extra_body")]
    public Dictionary<string, object>? ExtraBody { get; set; }

    public string? GetEntityId()
    {
        if (ExtraBody?.TryGetValue("entity_id", out var entityId) == true)
        {
            return entityId?.ToString();
        }
        return null;
    }

    public string GetLastMessageContent()
    {
        return Messages?.LastOrDefault()?.Content ?? "";
    }
}

public class DevUIRequestMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";

    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
}