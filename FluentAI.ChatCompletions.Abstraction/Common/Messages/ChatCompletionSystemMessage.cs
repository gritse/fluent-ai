namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionSystemMessage(string Content) : IChatCompletionMessage;