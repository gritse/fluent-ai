using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FluentAI.ChatCompletions.Abstraction.Tools;
using FluentAI.ChatCompletions.OpenAI;

var openAiToken = Environment.GetEnvironmentVariable("OPEN_AI_TOKEN")!;

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