using Azure.AI.OpenAI;
using FluentAI.ChatCompletions.Abstraction;
using FluentAI.ChatCompletions.Abstraction.Common;
using FluentAI.ChatCompletions.Abstraction.Common.Messages;
using FluentAI.ChatCompletions.Abstraction.Common.Tools;
using ChatCompletionsOptions = FluentAI.ChatCompletions.Abstraction.Common.ChatCompletionsOptions;
using OpenAIChatCompletionsOptions = Azure.AI.OpenAI.ChatCompletionsOptions;

namespace FluentAI.ChatCompletions.OpenAI;

public class ChatCompletionsOpenAiClient(OpenAIClient openAiClient) : IChatCompletionsClient
{
    public ChatCompletionsOpenAiClient(string openAiApiKey) : this(new OpenAIClient(openAiApiKey))
    { }

    public async Task<ChatCompletionsResponse> GetChatCompletionsAsync(ChatCompletionsOptions completionOptions)
    {
        var response = await openAiClient.GetChatCompletionsAsync(Map(completionOptions));
        var chatChoice = response.Value.Choices[0];

        var toolCalls = chatChoice.Message.ToolCalls?
                            .OfType<ChatCompletionsFunctionToolCall>()
                            .Select(Map)
                            .ToList() ?? new();

        return new ChatCompletionsResponse(
            IsChatToolCall: chatChoice.FinishReason == CompletionsFinishReason.ToolCalls,
            CompletionMessage: new ChatCompletionsAssistantMessage(chatChoice.Message.Content, toolCalls));
    }

    /// <summary>
    /// TODO
    /// </summary>
    /// <returns></returns>
    public ChatCompletionsBuilder ToCompletionsBuilder() =>
        new ChatCompletionsBuilder(new ChatCompletionsExecutor(this));

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
            ChatCompletionsFormat.Json => ChatCompletionsResponseFormat.JsonObject,
            ChatCompletionsFormat.Text => ChatCompletionsResponseFormat.Text
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

    private ChatRequestMessage Map(IChatCompletionsMessage message)
    {
        return message switch
        {
            ChatCompletionsUserMessage => new ChatRequestUserMessage(message.Content),
            ChatCompletionsAssistantMessage a => Map(a),
            ChatCompletionsSystemMessage => new ChatRequestSystemMessage(message.Content),
            ChatCompletionsToolMessage t => new ChatRequestToolMessage(message.Content, t.Id)
        };
    }

    private ChatRequestMessage Map(ChatCompletionsAssistantMessage message)
    {
        var assistantMessage = new ChatRequestAssistantMessage(message.Content);
        foreach (var toolCall in message.ToolCalls)
            assistantMessage.ToolCalls.Add(Map(toolCall));

        return assistantMessage;
    }
}