using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAI.ChatCompletions.Abstraction.Common.Messages;
using FluentAI.ChatCompletions.Abstraction.Common.Tools;
using FluentAI.ChatCompletions.Abstraction.Tools;
using FluentAI.ChatCompletions.Abstraction.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NJsonSchema;

namespace FluentAI.ChatCompletions.Abstraction.Common;

public class ChatCompletionExecutor(IChatCompletionsClient client)
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
            var unvalidatedJson = response.CompletionMessage.Content;

            if (unvalidatedJson.IsValidJson(responseSchema, out var errorMessage))
                return JObject.Parse(unvalidatedJson);

            chatCompletionsOptions.Messages.Add(new ChatCompletionUserMessage($"Answer isn't valid due to json schema validation errors:\r\n{errorMessage}"));

            remainingRetries--;
        }

        // Final attempt after the retries
        var finalResponse = await GetChatCompletionsAsync(chatCompletionsOptions, toolbox);
        var finalUnvalidatedJson = finalResponse.CompletionMessage.Content;

        if (finalUnvalidatedJson.IsValidJson(responseSchema, out var finalErrorMessage))
            return JObject.Parse(finalUnvalidatedJson);

        throw new InvalidOperationException($"Chat completion isn't valid due to json schema validation errors: {finalErrorMessage}");

    }

    public async Task<ChatCompletionResponse> GetChatCompletionsAsync(ChatCompletionsOptions chatCompletionsOptions, IReadOnlyDictionary<string, IChatCompletionTool> toolbox)
    {
        var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

        while (response.IsChatToolCall)
        {
            chatCompletionsOptions.Messages.Add(response.CompletionMessage);
            foreach (var toolCall in response.CompletionMessage.ToolCalls)
            {
                chatCompletionsOptions.Messages.Add(await GetToolCallResponseMessage(toolCall, toolbox));
            }

            response = await client.GetChatCompletionsAsync(chatCompletionsOptions);
        }

        return response;
    }

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

        if (!unvalidatedArguments.IsValidJson(requestSchema, out var errorMessage, ", "))
        {
            throw new ArgumentException($"Arguments for tool call validation failed: {errorMessage}");
        }

        var request = JsonConvert.DeserializeObject(unvalidatedArguments, requestType, serializerSettings);

        if (request == null)
        {
            throw new ArgumentException("Deserialization resulted in a null request.");
        }

        var response = await tool.Handle(request);

        return new ChatCompletionToolMessage(JsonConvert.SerializeObject(response, serializerSettings), toolCall.Id);
    }
}