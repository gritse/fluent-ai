using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.AI.OpenAI;
using FluentAI.Extensions;
using FluentAI.Tools;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NJsonSchema;

namespace FluentAI;

public class ChatCompletionsRequest(
    OpenAIClient openAiClient,
    JsonSchema? responseSchema,
    ChatCompletionsOptions chatCompletionsOptions,
    IReadOnlyDictionary<string, IChatCompletionTool> toolbox)
{
    private readonly JsonSerializer _jsonSerializer = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    /// <summary>
    /// Executes the chat completion request and returns the plain string response.
    /// </summary>
    /// <returns>The plain string response from the chat completion.</returns>
    public async Task<string> GetPlainStringResponse()
    {
        var response = await openAiClient.GetChatCompletionsAsync(chatCompletionsOptions.DeepClone(), toolbox);
        return response.Value.Choices[0].Message.Content;
    }

    /// <summary>
    /// Executes the chat completion request and returns the structured response of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// <returns>The structured response from the chat completion.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the response schema is not specified.</exception>
    public async Task<T> GetStructuredResponse<T>()
    {
        if (responseSchema is null)
        {
            throw new InvalidOperationException("Schema is not specified. Please use the UseResponseSchema<T>() method to define the response schema before calling GetStructuredResponse<T>().");
        }

        var response = await openAiClient.GetStructuredChatCompletionsAsync(chatCompletionsOptions.DeepClone(), responseSchema, toolbox);
        return response.ToObject<T>(_jsonSerializer) ?? throw new InvalidOperationException();
    }
}