using Microsoft.Agents.Workflows;

namespace Microsoft.Agents.DevUI.Samples;

/// <summary>
/// Simple workflow example
/// </summary>
public class SimpleWorkflow : Workflow<string>
{
    public SimpleWorkflow() : base("start_executor")
    {
    }

    public string ProcessInput(string input)
    {
        return $"Processed: {input.ToUpperInvariant()}";
    }
}