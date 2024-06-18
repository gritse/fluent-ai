using FluentAI.ChatCompletions.Abstraction.Common.Messages;

namespace FluentAI.ChatCompletions.Abstraction.Common;

public record ChatCompletionResponse(ChatCompletionAssistantMessage CompletionMessage, bool IsChatToolCall);