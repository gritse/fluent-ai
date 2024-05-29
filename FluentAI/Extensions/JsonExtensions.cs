using System.ClientModel.Primitives;
using System.Collections.Generic;
using Azure.AI.OpenAI;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Validation;

namespace FluentAI.Extensions;

internal static class JsonExtensions
{
    public static bool IsValidJson(this JObject obj, JsonSchema schema, out ICollection<ValidationError> validationErrors)
    {
        validationErrors = schema.Validate(obj);
        return validationErrors.Count == 0;
    }

    public static ChatCompletionsOptions DeepClone(this ChatCompletionsOptions options)
    {
        var binaryData = ((IPersistableModel<ChatCompletionsOptions>)options).Write(ModelReaderWriterOptions.Json);
        return ((IPersistableModel<ChatCompletionsOptions>)new ChatCompletionsOptions()).Create(binaryData, ModelReaderWriterOptions.Json);
    }
}