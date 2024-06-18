namespace FluentAI.ChatCompletions.Abstraction.Tools;

/// <summary>
/// Base class for chat completion tools, providing common functionality for handling requests and responses.
/// </summary>
/// <typeparam name="TRequest">The type of the request handled by the tool.</typeparam>
/// <typeparam name="TResponse">The type of the response returned by the tool.</typeparam>
/// <param name="functionName">The name of the function associated with the tool.</param>
public abstract class ChatCompletionToolBase<TRequest, TResponse>(string functionName) : IChatCompletionTool
{
    /// <summary>
    /// Gets or sets the name of the function associated with the tool.
    /// </summary>
    public string FunctionName { get; set; } = functionName;

    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    public abstract Task<TResponse> Handle(TRequest request);

    /// <summary>
    /// Handles the specified request and returns a response.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response.</returns>
    async Task<object?> IChatCompletionTool.Handle(object request) => await Handle((TRequest)request);

    /// <summary>
    /// Gets the type of the request handled by the tool.
    /// </summary>
    Type IChatCompletionTool.RequestType => typeof(TRequest);
}