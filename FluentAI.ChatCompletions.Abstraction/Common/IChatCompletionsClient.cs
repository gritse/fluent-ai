namespace FluentAI.ChatCompletions.Abstraction.Common;

public interface IChatCompletionsClient
{
    Task<ChatCompletionResponse> GetChatCompletionsAsync(ChatCompletionsOptions completionOptions);
}