using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FluentAI.ChatCompletions.Abstraction.Tools;

/// <summary>
/// Handles requests to fetch content from a specified URL.
/// </summary>
[Description("Gets content by specified url")]
public class FetchUrlTool() : ChatCompletionToolBase<FetchUrlTool.FetchUrlToolRequest, FetchUrlTool.FetchUrlToolResponse>("fetch_url")
{
    /// <summary>
    /// Handles the specified request to fetch content from a URL.
    /// </summary>
    /// <param name="request">The request containing the URL to fetch.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the response with the fetched content.</returns>
    public override async Task<FetchUrlToolResponse> Handle(FetchUrlToolRequest request)
    {
        var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(request.Url);
        return new FetchUrlToolResponse() { Content = response };
    }

    /// <summary>
    /// Represents a request to fetch the content from a specified URL.
    /// </summary>
    public class FetchUrlToolRequest
    {
        /// <summary>
        /// Gets or sets the URL to fetch.
        /// </summary>
        [Description("The URL"), Required, Url]
        public string Url { get; set; }
    }

    /// <summary>
    /// Represents the response containing the content fetched from a URL.
    /// </summary>
    public class FetchUrlToolResponse
    {
        /// <summary>
        /// Gets or sets the content fetched from the URL.
        /// </summary>
        public string Content { get; set; }
    }
}