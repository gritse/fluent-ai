using System.Collections.Generic;
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

    public static bool IsValidJson(this string json, JsonSchema schema, out ICollection<ValidationError> validationErrors)
    {
        validationErrors = schema.Validate(json);
        return validationErrors.Count == 0;
    }
}