using System.Collections.Generic;
using Azure.AI.OpenAI;
using FluentAI.Extensions;
using FluentAI.Tools;
using Newtonsoft.Json;
using JsonSchema = NJsonSchema.JsonSchema;

namespace FluentAI;

/// <summary>
/// Builder class for constructing and executing chat completions using the OpenAI API.
/// </summary>
/// <param name="openAiClient">The OpenAI client instance used to send requests.</param>
/// <param name="chatCompletionsOptions">Optional chat completion options to customize the requests.</param>
public class ChatCompletionsBuilder(OpenAIClient openAiClient, ChatCompletionsOptions? chatCompletionsOptions = null)
{
    private readonly Dictionary<string, IChatCompletionTool> _toolbox = new();
    private JsonSchema? _responseSchema;

    /// <summary>
    /// Gets or sets the chat completion options used to customize the chat completion requests.
    /// </summary>
    /// <value>
    /// An instance of <see cref="ChatCompletionsOptions"/> that contains the configuration for the chat completion requests.
    /// </value>
    public ChatCompletionsOptions ChatCompletionOptions { get; set; } = chatCompletionsOptions ?? new();

    /// <summary>
    /// Configures the builder to use the GPT-3.5-turbo model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseChatGpt35Turbo()
    {
        ChatCompletionOptions.DeploymentName = "gpt-3.5-turbo";
        return this;
    }

    /// <summary>
    /// Configures the builder to use the GPT-4-turbo model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseChatGpt4Turbo()
    {
        ChatCompletionOptions.DeploymentName = "gpt-4-turbo";
        return this;
    }

    /// <summary>
    /// Configures the builder to use the GPT-4o model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseChatGpt4o()
    {
        ChatCompletionOptions.DeploymentName = "gpt-4o";
        return this;
    }

    /// <summary>
    /// Configures the builder to use a specified chat model for completions.
    /// </summary>
    /// <param name="model">The name of the model to use.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseChatModel(string model)
    {
        ChatCompletionOptions.DeploymentName = model;
        return this;
    }

    /// <summary>
    /// Adds a system prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The system prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder SystemPrompt(string prompt)
    {
        ChatCompletionOptions.Messages.Add(new ChatRequestSystemMessage(prompt));
        return this;
    }

    /// <summary>
    /// Adds a user prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The user prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UserPrompt(string prompt)
    {
        ChatCompletionOptions.Messages.Add(new ChatRequestUserMessage(prompt));
        return this;
    }

    /// <summary>
    /// Adds a chat tool to the chat completion options.
    /// </summary>
    /// <param name="chatTool">The chat tool to add.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseChatTool(IChatCompletionTool chatTool)
    {
        var toolDefinition = chatTool.CreateToolDefinitionFromType();
        ChatCompletionOptions.Tools.Add(toolDefinition);

        _toolbox.Add(chatTool.FunctionName, chatTool);
        return this;
    }

    /// <summary>
    /// Configures the response schema for structured responses.
    /// </summary>
    /// <typeparam name="T">The type used to generate the response schema.</typeparam>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseResponseSchema<T>() =>
        UseResponseSchema(typeof(T).CreateSchemaFromType());

    /// <summary>
    /// Configures the response schema for structured responses using a JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema defining the structure of the response.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UseResponseSchema(JsonSchema jsonSchema)
    {
        _responseSchema = jsonSchema;

        ChatCompletionOptions.ResponseFormat = ChatCompletionsResponseFormat.JsonObject;
        ChatCompletionOptions.Messages.Add(new ChatRequestUserMessage($"Return answer in json format as specified in schema:\r\n{_responseSchema.ToJson(Formatting.Indented)}"));

        return this;
    }

    /// <summary>
    /// Builds the chat completions request using the configured options.
    /// </summary>
    /// <returns>A <see cref="ChatCompletionsRequest"/> instance representing the configured chat completions request.</returns>
    public ChatCompletionsRequest BuildCompletionsRequest() => new(openAiClient, _responseSchema, ChatCompletionOptions, _toolbox);
}