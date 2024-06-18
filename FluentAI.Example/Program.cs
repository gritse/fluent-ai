using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using FluentAI.ChatCompletions.Abstraction.Tools;
using FluentAI.ChatCompletions.OpenAI;
using FluentAI.Example;

var openAiToken = Keychain.GetKeychainPassword("OPEN_AI_TOKEN", Environment.UserName);

var request = new ChatCompletionOpenAiClient(openAiToken)
    .ToCompletionsBuilder()
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