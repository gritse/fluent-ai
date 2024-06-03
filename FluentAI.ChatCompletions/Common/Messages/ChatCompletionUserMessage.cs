namespace FluentAI.ChatCompletions.Common.Messages;

public record ChatCompletionUserMessage(string Content) : IChatCompletionMessage;