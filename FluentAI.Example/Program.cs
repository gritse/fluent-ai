using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Azure.AI.OpenAI;
using FluentAI.Example;
using FluentAI;
using FluentAI.Tools;

var openAiToken = Keychain.GetKeychainPassword("OPEN_AI_TOKEN", Environment.UserName);
var client = new OpenAIClient(openAiToken);

var request = new ChatCompletionsBuilder(client)
    .UseChatModel("gpt-4o")
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