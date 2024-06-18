namespace FluentAI.ChatCompletions.Abstraction;

public interface IChatCompletionsRequest
{
    /// <summary>
    /// Executes the chat completion request and returns the plain string response.
    /// </summary>
    /// <returns>The plain string response from the chat completion.</returns>
    Task<string> GetPlainStringResponse();

    /// <summary>
    /// Executes the chat completion request and returns the structured response of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to deserialize the response into.</typeparam>
    /// /// <param name="retryCount">The number of times to retry the request in case of failure.</param>
    /// <returns>The structured response from the chat completion.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the response schema is not specified.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the response fails validation after the maximum number of retries.</exception>
    Task<T> GetStructuredResponse<T>(int retryCount = 2);
}