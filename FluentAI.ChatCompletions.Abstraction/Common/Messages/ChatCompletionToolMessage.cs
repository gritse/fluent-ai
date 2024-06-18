namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionToolMessage(string Content, string Id) : IChatCompletionMessage;