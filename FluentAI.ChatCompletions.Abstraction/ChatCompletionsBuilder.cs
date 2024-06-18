using System.Collections.Generic;
using FluentAI.ChatCompletions.Abstraction.Common;
using FluentAI.ChatCompletions.Abstraction.Common.Messages;
using FluentAI.ChatCompletions.Abstraction.Tools;
using FluentAI.ChatCompletions.Abstraction.Extensions;
using Newtonsoft.Json;
using JsonSchema = NJsonSchema.JsonSchema;

namespace FluentAI.ChatCompletions.Abstraction;

/// <summary>
/// Builder class for constructing and executing chat completions using the OpenAI API.
/// </summary>
/// <param name="chatCompletionExecutor">The OpenAI client instance used to send requests.</param>
/// <param name="chatCompletionsOptions">Optional chat completion options to customize the requests.</param>
public class ChatCompletionsBuilder(ChatCompletionExecutor chatCompletionExecutor, ChatCompletionsOptions? chatCompletionsOptions = null) : IChatCompletionsBuilder
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
        ChatCompletionOptions.Messages.Add(new ChatCompletionSystemMessage(prompt));
        return this;
    }

    /// <summary>
    /// Adds a assistant prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The assistant prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder AssistantPrompt(string prompt)
    {
        ChatCompletionOptions.Messages.Add(new ChatCompletionAssistantMessage(prompt, new()));
        return this;
    }

    /// <summary>
    /// Adds a user prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The user prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public ChatCompletionsBuilder UserPrompt(string prompt)
    {
        ChatCompletionOptions.Messages.Add(new ChatCompletionUserMessage(prompt));
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

        ChatCompletionOptions.ResponseFormat = ChatCompletionFormat.Json;
        ChatCompletionOptions.Messages.Add(new ChatCompletionUserMessage($"Return answer in json format as specified in schema:\r\n{_responseSchema.ToJson(Formatting.Indented)}"));

        return this;
    }

    /// <summary>
    /// Builds the chat completions request using the configured options.
    /// </summary>
    /// <returns>A <see cref="ChatCompletionsRequest"/> instance representing the configured chat completions request.</returns>
    public ChatCompletionsRequest BuildCompletionsRequest() => new(chatCompletionExecutor, _responseSchema, ChatCompletionOptions, _toolbox);
}