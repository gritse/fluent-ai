using Azure.AI.OpenAI;
using FluentAI.ChatCompletions.Abstraction;
using FluentAI.ChatCompletions.Abstraction.Common;
using FluentAI.ChatCompletions.Abstraction.Common.Messages;
using FluentAI.ChatCompletions.Abstraction.Common.Tools;
using ChatCompletionsOptions = FluentAI.ChatCompletions.Abstraction.Common.ChatCompletionsOptions;
using OpenAIChatCompletionsOptions = Azure.AI.OpenAI.ChatCompletionsOptions;

namespace FluentAI.ChatCompletions.OpenAI;

public class ChatCompletionOpenAiClient(OpenAIClient openAiClient) : IChatCompletionsClient
{
    public ChatCompletionOpenAiClient(string openAiApiKey) : this(new OpenAIClient(openAiApiKey))
    { }

    public async Task<ChatCompletionResponse> GetChatCompletionsAsync(ChatCompletionsOptions completionOptions)
    {
        var response = await openAiClient.GetChatCompletionsAsync(Map(completionOptions));
        var chatChoice = response.Value.Choices[0];

        var toolCalls = chatChoice.Message.ToolCalls?
                            .OfType<ChatCompletionsFunctionToolCall>()
                            .Select(Map)
                            .ToList() ?? new();

        return new ChatCompletionResponse(
            IsChatToolCall: chatChoice.FinishReason == CompletionsFinishReason.ToolCalls,
            CompletionMessage: new ChatCompletionAssistantMessage(chatChoice.Message.Content, toolCalls));
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    public ChatCompletionsBuilder ToCompletionsBuilder() =>
        new ChatCompletionsBuilder(new ChatCompletionExecutor(this));

    private ChatCompletionsFunctionCall Map(ChatCompletionsFunctionToolCall options)
    {
        return new ChatCompletionsFunctionCall()
        {
            Id = options.Id,
            Name = options.Name,
            Arguments = options.Arguments
        };
    }

    private ChatCompletionsFunctionToolCall Map(ChatCompletionsFunctionCall options)
    {
        return new ChatCompletionsFunctionToolCall(options.Id, options.Name, options.Arguments);
    }

    private OpenAIChatCompletionsOptions Map(ChatCompletionsOptions options)
    {
        var openAiChatCompletionOptions = new OpenAIChatCompletionsOptions
        {
            DeploymentName = options.DeploymentName
        };

        foreach (var tool in options.Tools)
            openAiChatCompletionOptions.Tools.Add(Map(tool));

        foreach (var message in options.Messages)
            openAiChatCompletionOptions.Messages.Add(Map(message));

        openAiChatCompletionOptions.ResponseFormat = options.ResponseFormat switch
        {
            ChatCompletionFormat.Json => ChatCompletionsResponseFormat.JsonObject,
            ChatCompletionFormat.Text => ChatCompletionsResponseFormat.Text
        };

        return openAiChatCompletionOptions;
    }

    private ChatCompletionsToolDefinition Map(IChatCompletionsFunctionDefinition options)
    {
        return new ChatCompletionsFunctionToolDefinition()
        {
            Name = options.Name,
            Description = options.Description,
            Parameters = BinaryData.FromString(options.Parameters.ToJson())
        };
    }

    private ChatRequestMessage Map(IChatCompletionMessage message)
    {
        return message switch
        {
            ChatCompletionUserMessage => new ChatRequestUserMessage(message.Content),
            ChatCompletionAssistantMessage a => Map(a),
            ChatCompletionSystemMessage => new ChatRequestSystemMessage(message.Content),
            ChatCompletionToolMessage t => new ChatRequestToolMessage(message.Content, t.Id)
        };
    }

    private ChatRequestMessage Map(ChatCompletionAssistantMessage message)
    {
        var assistantMessage = new ChatRequestAssistantMessage(message.Content);
        foreach (var toolCall in message.ToolCalls)
            assistantMessage.ToolCalls.Add(Map(toolCall));

        return assistantMessage;
    }
}