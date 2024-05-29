using System;
using System.Threading.Tasks;

namespace FluentAI.Tools;

public interface IChatCompletionTool
{
    string FunctionName { get; }

    Task<object> Handle(object request);

    Type RequestType { get; }
}