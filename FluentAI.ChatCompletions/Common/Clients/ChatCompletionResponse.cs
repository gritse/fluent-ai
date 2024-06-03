using FluentAI.ChatCompletions.Common.Messages;

namespace FluentAI.ChatCompletions.Common.Clients;

public class ChatCompletionResponse
{
    public ChatCompletionAssistantMessage CompletionMessage { get; init; }

    public bool IsChatToolCall { get; init; }
}