# Fluent AI

[![NuGet version (FluentAI.ChatCompletions.OpenAI)](https://img.shields.io/nuget/v/FluentAI.ChatCompletions.OpenAI.svg?style=flat-square)](https://www.nuget.org/packages/FluentAI.ChatCompletions.OpenAI/)


Fluent AI is a powerful library designed to build and execute advanced prompts using OpenAI's various ChatGPT models. It leverages the capabilities of [Azure.AI.OpenAI](https://www.nuget.org/packages/Azure.AI.OpenAI) and [NJsonSchema](https://www.nuget.org/packages/NJsonSchema) packages to provide a seamless experience for integrating AI into your .NET applications.

## Features

- **Fluent Building of User and System Prompts:** Easily construct prompts for ChatGPT models.
- **Custom Chat Tools Integration:** Integrate custom chat functions to enable ChatGPT to call arbitrary .NET methods and utilize returned values.
- **Structured Responses:** Automatically instruct the model to respond in a specified JSON format.
- **Built-in Validation:** Validate ChatGPT responses using JSON Schema and auto-retry in case the model returns incorrectly formatted answers.

## Example Usage

```csharp
var openAiToken = "...";

var client = new ChatCompletionsOpenAiClient(openAiToken);
var builder = client.ToCompletionsBuilder();

var request = builder
    .UseChatGpt4o()
    .UseChatTool(new FetchUrlTool())
    .UserPrompt("Give me short description of the following webpage: https://docs.bland.ai/welcome-to-bland")
    .UseResponseSchema<ChatGptResponse>()
    .BuildCompletionsRequest();

var response = await request.GetStructuredResponse<ChatGptResponse>();

Console.WriteLine(response.Text);

[Description("This is response model you should use to send answer for questions")]
public class ChatGptResponse
{
    [Description("Your response message"), Required]
    public string Text { get; set; }
}
```

## Implementation of FetchUrlTool

```csharp
/// <summary>
/// Handles requests to fetch content from a specified URL.
/// </summary>
[Description("Gets content by specified URL")]
public class FetchUrlTool : ChatCompletionToolBase<FetchUrlTool.FetchUrlToolRequest, FetchUrlTool.FetchUrlToolResponse>
{
    public FetchUrlTool() : base("fetch_url") {}

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
    /// Represents a request to fetch content from a specified URL.
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
```

## Important Note

The DescriptionAttribute is mandatory as it describes the meaning of the properties to the model, ensuring that the AI understands the context and purpose of each property.

## Contributing

Contributions are welcome! Please feel free to submit a pull request or open an issue.

## License

This project is licensed under the MIT License. See the LICENSE file for more details.