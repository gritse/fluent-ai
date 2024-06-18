namespace FluentAI.ChatCompletions.Abstraction.Common.Tools;

public class ChatCompletionsFunctionCall
{
    public string Id { get; init; }
    public string Arguments { get; init; }
    public string Name { get; init; }
}