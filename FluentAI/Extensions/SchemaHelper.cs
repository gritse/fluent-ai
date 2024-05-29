using System;
using System.ComponentModel;
using System.Reflection;
using System.Text.Json;
using Azure.AI.OpenAI;
using FluentAI.Tools;
using NJsonSchema;
using NJsonSchema.Generation;

namespace FluentAI.Extensions;

internal static class SchemaHelper
{
    public static ChatCompletionsFunctionToolDefinition CreateToolDefinitionFromType<T>(this T tool) where T : IChatCompletionTool
    {
        var type = typeof(T);
        var descriptionAttribute = type.GetCustomAttribute<DescriptionAttribute>();

        var name = tool.FunctionName;
        var description = descriptionAttribute?.Description ?? string.Empty;
        var parameters = CreateSchemaFromType(tool.RequestType);

        return new ChatCompletionsFunctionToolDefinition
        {
            Name = name,
            Description = description,
            Parameters = BinaryData.FromString(parameters.ToJson())
        };
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