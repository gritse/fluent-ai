namespace FluentAI.ChatCompletions.Abstraction.Tools;

public interface IChatCompletionsTool
{
    string FunctionName { get; }

    Task<object?> Handle(object request);

    Type RequestType { get; }
}