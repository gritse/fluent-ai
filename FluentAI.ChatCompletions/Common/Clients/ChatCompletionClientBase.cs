using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAI.ChatCompletions.Common.Messages;
using FluentAI.ChatCompletions.Common.Tools;
using FluentAI.ChatCompletions.Tools;
using FluentAI.ChatCompletions.Extensions;
using FluentAI.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;

namespace FluentAI.ChatCompletions.Common.Clients;

public abstract class ChatCompletionClientBase
{
    public async Task<JObject> GetStructuredChatCompletionsAsync(ChatCompletionsOptions chatCompletionsOptions,
        JsonSchema responseSchema,
        IReadOnlyDictionary<string, IChatCompletionTool>? toolbox = null,
        int maxRetries = 2)
    {
        toolbox ??= new Dictionary<string, IChatCompletionTool>();

        int remainingRetries = maxRetries;

        while (remainingRetries > 0)
        {
            var response = await GetChatCompletionsAsync(chatCompletionsOptions, toolbox);
            var unvalidatedJson = JObject.Parse(response.CompletionMessage.Content);

            if (unvalidatedJson.IsValidJson(responseSchema, out var errorMessages))
                return unvalidatedJson;

            var errors = string.Join("\r\n", errorMessages);
            chatCompletionsOptions.Messages.Add(new ChatCompletionUserMessage($"Answer isn't valid due to json schema validation errors:\r\n{errors}"));

            remainingRetries--;
        }

        // Final attempt after the retries
        var finalResponse = await GetChatCompletionsAsync(chatCompletionsOptions, toolbox);
        var finalUnvalidatedJson = JObject.Parse(finalResponse.CompletionMessage.Content);

        if (finalUnvalidatedJson.IsValidJson(responseSchema, out var finalErrorMessages))
            return finalUnvalidatedJson;

        var finalErrors = string.Join(", ", finalErrorMessages);
        throw new InvalidOperationException($"Chat completion isn't valid due to json schema validation errors: {finalErrors}");

    }

    public async Task<ChatCompletionResponse> GetChatCompletionsAsync(ChatCompletionsOptions chatCompletionsOptions, IReadOnlyDictionary<string, IChatCompletionTool> toolbox)
    {
        var response = await GetChatCompletionsAsync(chatCompletionsOptions);

        while (response.IsChatToolCall)
        {
            chatCompletionsOptions.Messages.Add(response.CompletionMessage);
            foreach (var toolCall in response.CompletionMessage.ToolCalls)
            {
                chatCompletionsOptions.Messages.Add(await GetToolCallResponseMessage(toolCall, toolbox));
            }

            response = await GetChatCompletionsAsync(chatCompletionsOptions);
        }

        return response;
    }

    protected abstract Task<ChatCompletionResponse> GetChatCompletionsAsync(ChatCompletionsOptions completionOptions);

    private static async Task<ChatCompletionToolMessage> GetToolCallResponseMessage(ChatCompletionsFunctionCall toolCall,
        IReadOnlyDictionary<string, IChatCompletionTool> toolbox)
    {
        var unvalidatedArguments = toolCall.Arguments;

        var serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        if (!toolbox.TryGetValue(toolCall.Name, out var tool))
        {
            throw new KeyNotFoundException($"No tool found with the name {toolCall.Name} in the toolbox.");
        }

        var requestType = tool.RequestType;

        var requestSchema = requestType.CreateSchemaFromType();

        var jsonArguments = JObject.Parse(unvalidatedArguments);
        if (!jsonArguments.IsValidJson(requestSchema, out var errors))
        {
            var errorMessages = string.Join(", ", errors);
            throw new ArgumentException($"Arguments for tool call validation failed: {errorMessages}");
        }

        var request = jsonArguments.ToObject(requestType, JsonSerializer.CreateDefault(serializerSettings));

        if (request == null)
        {
            throw new ArgumentException("Deserialization resulted in a null request.");
        }

        var response = await tool.Handle(request);

        return new ChatCompletionToolMessage(JsonConvert.SerializeObject(response, serializerSettings), toolCall.Id);
    }
}