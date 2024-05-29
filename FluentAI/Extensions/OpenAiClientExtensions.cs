using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using FluentAI.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using JsonSchema = NJsonSchema.JsonSchema;

namespace FluentAI.Extensions;

/// <summary>
/// Extension methods for the <see cref="OpenAIClient"/> class, providing additional functionality for handling chat completions.
/// </summary>
public static class OpenAiClientExtensions
{
    /// <summary>
    /// Gets structured chat completions with JSON schema validation and retry logic.
    /// </summary>
    /// <param name="openAiClient">The OpenAI client instance used to send requests.</param>
    /// <param name="chatCompletionsOptions">The options for the chat completions request.</param>
    /// <param name="responseSchema">The JSON schema to validate the response against.</param>
    /// <param name="toolbox">An optional toolbox of chat completion tools.</param>
    /// <param name="maxRetries">The maximum number of retries if validation fails.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the validated JSON response.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the response fails validation after the maximum number of retries.</exception>
    public static async Task<JObject> GetStructuredChatCompletionsAsync(this OpenAIClient openAiClient,
        ChatCompletionsOptions chatCompletionsOptions,
        JsonSchema responseSchema,
        IReadOnlyDictionary<string, IChatCompletionTool>? toolbox = null,
        int maxRetries = 2)
    {
        toolbox ??= new Dictionary<string, IChatCompletionTool>();

        int remainingRetries = maxRetries;

        while (remainingRetries > 0)
        {
            var response = await openAiClient.GetChatCompletionsAsync(chatCompletionsOptions, toolbox);
            var unvalidatedJson = JObject.Parse(response.Value.Choices[0].Message.Content);

            if (unvalidatedJson.IsValidJson(responseSchema, out var errorMessages))
                return unvalidatedJson;

            var errors = string.Join("\r\n", errorMessages);
            chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage($"Answer isn't valid due to json schema validation errors:\r\n{errors}"));

            remainingRetries--;
        }

        // Final attempt after the retries
        var finalResponse = await openAiClient.GetChatCompletionsAsync(chatCompletionsOptions, toolbox);
        var finalUnvalidatedJson = JObject.Parse(finalResponse.Value.Choices[0].Message.Content);

        if (finalUnvalidatedJson.IsValidJson(responseSchema, out var finalErrorMessages))
            return finalUnvalidatedJson;

        var finalErrors = string.Join(", ", finalErrorMessages);
        throw new InvalidOperationException($"Chat completion isn't valid due to json schema validation errors: {finalErrors}");
    }

    /// <summary>
    /// Gets chat completions, handling tool calls if necessary.
    /// </summary>
    /// <param name="openAiClient">The OpenAI client instance used to send requests.</param>
    /// <param name="chatCompletionsOptions">The options for the chat completions request.</param>
    /// <param name="toolbox">A dictionary of tools to handle tool calls.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the chat completions response.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if a tool call references a tool that is not found in the toolbox.</exception>
    public static async Task<Response<ChatCompletions>> GetChatCompletionsAsync(this OpenAIClient openAiClient,
        ChatCompletionsOptions chatCompletionsOptions,
        IReadOnlyDictionary<string, IChatCompletionTool> toolbox)
    {
        var response = await openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);

        while (response.Value.Choices[0].FinishReason == CompletionsFinishReason.ToolCalls)
        {
            var responseChoice = response.Value.Choices[0];
            ChatRequestAssistantMessage toolCallHistoryMessage = new(responseChoice.Message);
            chatCompletionsOptions.Messages.Add(toolCallHistoryMessage);

            foreach (var toolCall in responseChoice.Message.ToolCalls)
            {
                chatCompletionsOptions.Messages.Add(await GetToolCallResponseMessage(toolCall, toolbox));
            }

            response = await openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);
        }

        return response;
    }

    /// <summary>
    /// Gets the response message for a tool call.
    /// </summary>
    /// <param name="toolCall">The tool call to handle.</param>
    /// <param name="toolbox">A dictionary of tools to handle the tool call.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the tool response message.</returns>
    /// <exception cref="KeyNotFoundException">Thrown if the tool call references a tool that is not found in the toolbox.</exception>
    /// <exception cref="ArgumentException">Thrown if the tool call arguments fail validation or deserialization.</exception>
    private static async Task<ChatRequestToolMessage> GetToolCallResponseMessage(ChatCompletionsToolCall toolCall,
        IReadOnlyDictionary<string, IChatCompletionTool> toolbox)
    {
        var functionToolCall = (ChatCompletionsFunctionToolCall)toolCall;
        var unvalidatedArguments = functionToolCall.Arguments;

        var serializerSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };

        if (!toolbox.TryGetValue(functionToolCall.Name, out var tool))
        {
            throw new KeyNotFoundException($"No tool found with the name {functionToolCall.Name} in the toolbox.");
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

        return new ChatRequestToolMessage(JsonConvert.SerializeObject(response, serializerSettings), toolCall.Id);
    }
}