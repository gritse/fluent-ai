using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Azure.AI.OpenAI;
using FluentAI.ChatCompletions;
using FluentAI.ChatCompletions.Common;
using FluentAI.ChatCompletions.OpenAI;
using FluentAI.ChatCompletions.Tools;
using FluentAI.Example;

var openAiToken = Keychain.GetKeychainPassword("OPEN_AI_TOKEN", Environment.UserName);
var client = new OpenAIClient(openAiToken);

var request = new ChatCompletionsBuilder(new ChatCompletionExecutor(new ChatCompletionOpenAIClient(client)))
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