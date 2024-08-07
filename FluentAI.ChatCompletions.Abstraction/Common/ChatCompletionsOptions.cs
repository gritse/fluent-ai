using FluentAI.ChatCompletions.Abstraction.Common.Messages;
using FluentAI.ChatCompletions.Abstraction.Common.Tools;

namespace FluentAI.ChatCompletions.Abstraction.Common;

public class ChatCompletionsOptions
{
    public List<IChatCompletionsMessage> Messages { get; private init; } = new();

    public List<IChatCompletionsFunctionDefinition> Tools { get; private init; } = new();
    public string DeploymentName { get; set; }
    public ChatCompletionsFormat ResponseFormat { get; set; }

    public ChatCompletionsOptions Clone()
    {
        return new ChatCompletionsOptions()
        {
            Messages = Messages.ToList(),
            Tools = Tools.ToList(),
            DeploymentName = DeploymentName,
            ResponseFormat = ResponseFormat
        };
    }
}