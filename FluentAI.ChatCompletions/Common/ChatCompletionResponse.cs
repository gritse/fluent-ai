using FluentAI.ChatCompletions.Common.Messages;

namespace FluentAI.ChatCompletions.Common;

public record ChatCompletionResponse(ChatCompletionAssistantMessage CompletionMessage, bool IsChatToolCall);