namespace FluentAI.ChatCompletions.Abstraction.Common;

public interface IChatCompletionsClient
{
    Task<ChatCompletionsResponse> GetChatCompletionsAsync(ChatCompletionsOptions completionOptions);
}