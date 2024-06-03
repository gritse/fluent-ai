namespace FluentAI.ChatCompletions.Common.Messages;

public record ChatCompletionToolMessage(string Content, string Id) : IChatCompletionMessage;