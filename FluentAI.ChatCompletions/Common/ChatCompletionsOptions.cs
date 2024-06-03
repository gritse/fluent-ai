using System.Collections.Generic;
using System.Linq;
using FluentAI.ChatCompletions.Common.Messages;
using FluentAI.ChatCompletions.Common.Tools;

namespace FluentAI.ChatCompletions.Common;

public class ChatCompletionsOptions
{
    public List<IChatCompletionMessage> Messages { get; private init; } = new();

    public List<IChatCompletionsFunctionDefinition> Tools { get; private init; } = new();
    public string DeploymentName { get; set; }
    public ChatCompletionFormat ResponseFormat { get; set; }

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