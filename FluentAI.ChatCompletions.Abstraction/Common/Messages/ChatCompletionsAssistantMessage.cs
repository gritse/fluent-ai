using FluentAI.ChatCompletions.Abstraction.Common.Tools;

namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionsAssistantMessage(string Content, List<ChatCompletionsFunctionCall> ToolCalls) : IChatCompletionsMessage;