using NJsonSchema;

namespace FluentAI.ChatCompletions.Common.Tools;

public interface IChatCompletionsFunctionDefinition
{
    public string Name { get; }

    public string Description { get; }

    public JsonSchema Parameters { get; }
}