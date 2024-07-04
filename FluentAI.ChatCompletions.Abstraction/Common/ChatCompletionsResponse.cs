using FluentAI.ChatCompletions.Abstraction.Common.Messages;

namespace FluentAI.ChatCompletions.Abstraction.Common;

public record ChatCompletionsResponse(ChatCompletionsAssistantMessage CompletionMessage, bool IsChatToolCall);