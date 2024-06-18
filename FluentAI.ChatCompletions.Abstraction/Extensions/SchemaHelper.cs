using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using FluentAI.ChatCompletions.Abstraction.Common;
using FluentAI.ChatCompletions.Abstraction.Common.Tools;
using FluentAI.ChatCompletions.Abstraction.Tools;
using NJsonSchema;
using NJsonSchema.Generation;

namespace FluentAI.ChatCompletions.Abstraction.Extensions;

internal static class SchemaHelper
{
    public static IChatCompletionsFunctionDefinition CreateToolDefinitionFromType<T>(this T tool) where T : IChatCompletionTool
    {
        var type = typeof(T);
        var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();

        var name = tool.FunctionName;
        var description = descriptionAttribute?.Description ?? string.Empty;
        var parameters = CreateSchemaFromType(tool.RequestType);

        return new ChatFunctionDefinition(name, description, parameters);
    }

    public static JsonSchema CreateSchemaFromType(this Type type)
    {
        var schema = JsonSchema.FromType(type, new SystemTextJsonSchemaGeneratorSettings()
        {
            SerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        });

        return schema;
    }
}