using Microsoft.Extensions.AI;
using Microsoft.Extensions.AI.Agents;
using System.Runtime.CompilerServices;

namespace Microsoft.Agents.DevUI.Samples;

/// <summary>
/// Simple weather agent example
/// </summary>
public class WeatherAgent : AIAgent
{
    public override string Id => "weather_agent";
    public override string Name => "Weather Agent";
    public override string Description => "Provides weather information for locations";

    public override async Task<AgentRunResponse> RunAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        // Mock weather response
        var lastMessage = messages.LastOrDefault()?.Text ?? "no location specified";
        var response = $"🌤️ Weather for '{lastMessage}': Sunny, 72°F. This is a mock response from the Weather Agent.";

        var chatMessage = new ChatMessage(ChatRole.Assistant, response);

        thread ??= GetNewThread();
        // Note: AgentThread doesn't have Messages property in this framework version
        // The messages are managed by the framework

        return new AgentRunResponse(chatMessage);
    }

    public override async IAsyncEnumerable<AgentRunResponseUpdate> RunStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentThread? thread = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lastMessage = messages.LastOrDefault()?.Text ?? "no location specified";
        var response = $"🌤️ Weather for '{lastMessage}': Sunny, 72°F. This is a mock streaming response from the Weather Agent.";

        // Simulate streaming by yielding character by character
        foreach (char c in response)
        {
            if (cancellationToken.IsCancellationRequested)
                yield break;

            yield return new AgentRunResponseUpdate(ChatRole.Assistant, c.ToString());

            await Task.Delay(50, cancellationToken); // Simulate network delay
        }
    }
}