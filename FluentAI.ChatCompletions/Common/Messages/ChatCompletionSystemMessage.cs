namespace FluentAI.ChatCompletions.Common.Messages;

public record ChatCompletionSystemMessage(string Content) : IChatCompletionMessage;