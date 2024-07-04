namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionsUserMessage(string Content) : IChatCompletionsMessage;