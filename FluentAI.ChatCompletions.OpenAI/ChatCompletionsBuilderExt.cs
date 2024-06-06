namespace FluentAI.ChatCompletions.OpenAI;

public static class ChatCompletionsBuilderExt
{
    /// <summary>
    /// Configures the builder to use the GPT-3.5-turbo model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public static ChatCompletionsBuilder UseChatGpt35Turbo(this ChatCompletionsBuilder builder)
    {
        builder.ChatCompletionOptions.DeploymentName = "gpt-3.5-turbo";
        return builder;
    }

    /// <summary>
    /// Configures the builder to use the GPT-4-turbo model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public static ChatCompletionsBuilder UseChatGpt4Turbo(this ChatCompletionsBuilder builder)
    {
        builder.ChatCompletionOptions.DeploymentName = "gpt-4-turbo";
        return builder;
    }

    /// <summary>
    /// Configures the builder to use the GPT-4o model for chat completions.
    /// </summary>
    /// <returns>The current instance of the <see cref="ChatCompletionsBuilder"/>.</returns>
    public static ChatCompletionsBuilder UseChatGpt4o(this ChatCompletionsBuilder builder)
    {
        builder.ChatCompletionOptions.DeploymentName = "gpt-4o";
        return builder;
    }
}