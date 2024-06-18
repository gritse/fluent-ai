using System.Collections.Generic;
using FluentAI.ChatCompletions.Abstraction.Common.Tools;

namespace FluentAI.ChatCompletions.Abstraction.Common.Messages;

public record ChatCompletionAssistantMessage(string Content, List<ChatCompletionsFunctionCall> ToolCalls) : IChatCompletionMessage;