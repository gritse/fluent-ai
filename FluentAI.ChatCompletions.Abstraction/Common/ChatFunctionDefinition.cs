using FluentAI.ChatCompletions.Abstraction.Common.Tools;
using NJsonSchema;

namespace FluentAI.ChatCompletions.Abstraction.Common;

public record ChatFunctionDefinition(string Name, string Description, JsonSchema Parameters) : IChatCompletionsFunctionDefinition;