namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionsToolMessage(string Content, string Id) : IChatCompletionsMessage;