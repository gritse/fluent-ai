namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionUserMessage(string Content) : IChatCompletionMessage;