using System.Threading.Tasks;

namespace FluentAI.ChatCompletions.Common;

public interface IChatCompletionsClient
{
    Task<ChatCompletionResponse> GetChatCompletionsAsync(ChatCompletionsOptions completionOptions);
}