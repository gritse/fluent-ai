namespace FluentAI.ChatCompletions.Common.Tools;

public class ChatCompletionsFunctionCall
{
    public string Id { get; init; }
    public string Arguments { get; init; }
    public string Name { get; init; }
}