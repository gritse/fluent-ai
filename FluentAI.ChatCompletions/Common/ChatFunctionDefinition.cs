using FluentAI.ChatCompletions.Common.Tools;
using NJsonSchema;

namespace FluentAI.ChatCompletions.Common;

public record ChatFunctionDefinition(string Name, string Description, JsonSchema Parameters) : IChatCompletionsFunctionDefinition;