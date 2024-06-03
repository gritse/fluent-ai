using System.Collections.Generic;
using FluentAI.ChatCompletions.Common.Tools;

namespace FluentAI.ChatCompletions.Common.Messages;

public record ChatCompletionAssistantMessage(string Content, List<ChatCompletionsFunctionCall> ToolCalls) : IChatCompletionMessage;