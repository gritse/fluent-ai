namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionsSystemMessage(string Content) : IChatCompletionsMessage;