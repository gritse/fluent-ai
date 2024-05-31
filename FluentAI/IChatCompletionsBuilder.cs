using Azure.AI.OpenAI;
using FluentAI.Tools;
using NJsonSchema;

namespace FluentAI;

public interface IChatCompletionsBuilder
{
    /// <summary>
    /// Gets or sets the chat completion options used to customize the chat completion requests.
    /// </summary>
    /// <value>
    /// An instance of <see cref="ChatCompletionsOptions"/> that contains the configuration for the chat completion requests.
    /// </value>
    ChatCompletionsOptions ChatCompletionOptions { get; set; }

    /// <summary>
    /// Configures the builder to use the GPT-3.5-turbo model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseChatGpt35Turbo();

    /// <summary>
    /// Configures the builder to use the GPT-4-turbo model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseChatGpt4Turbo();

    /// <summary>
    /// Configures the builder to use the GPT-4o model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseChatGpt4o();

    /// <summary>
    /// Configures the builder to use a specified chat model for completions.
    /// </summary>
    /// <param name="model">The name of the model to use.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseChatModel(string model);

    /// <summary>
    /// Adds a system prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The system prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder SystemPrompt(string prompt);

    /// <summary>
    /// Adds a assistant prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The assistant prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder AssistantPrompt(string prompt);

    /// <summary>
    /// Adds a user prompt message to the chat completion options.
    /// </summary>
    /// <param name="prompt">The user prompt message.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UserPrompt(string prompt);

    /// <summary>
    /// Adds a chat tool to the chat completion options.
    /// </summary>
    /// <param name="chatTool">The chat tool to add.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseChatTool(IChatCompletionTool chatTool);

    /// <summary>
    /// Configures the response schema for structured responses.
    /// </summary>
    /// <typeparam name="T">The type used to generate the response schema.</typeparam>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseResponseSchema<T>();

    /// <summary>
    /// Configures the response schema for structured responses using a JSON schema.
    /// </summary>
    /// <param name="jsonSchema">The JSON schema defining the structure of the response.</param>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    ChatCompletionsBuilder UseResponseSchema(JsonSchema jsonSchema);

    /// <summary>
    /// Builds the chat completions request using the configured options.
    /// </summary>
    /// <returns>A <see cref="ChatCompletionsRequest"/> instance representing the configured chat completions request.</returns>
    ChatCompletionsRequest BuildCompletionsRequest();
}