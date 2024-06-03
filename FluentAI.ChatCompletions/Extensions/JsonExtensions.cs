using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NJsonSchema;

namespace FluentAI.ChatCompletions.Extensions;

internal static class JsonExtensions
{
    public static bool IsValidJson(this string unvalidatedJson, JsonSchema schema, out string validationErrorMessage, string separator = "\r\n")
    {
        JObject json;
        try
        {
            json = JObject.Parse(unvalidatedJson);
        }
        catch (JsonReaderException exception)
        {
            validationErrorMessage = exception.Message;
            return false;
        }

        var validationErrors = schema.Validate(json);
        validationErrorMessage = string.Join(separator, validationErrors);

        return validationErrors.Count == 0;
    }
}