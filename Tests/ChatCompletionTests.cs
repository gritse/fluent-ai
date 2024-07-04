using System.Text.Json;
using FluentAI.ChatCompletions.Abstraction.Common;
using FluentAI.ChatCompletions.Abstraction.Common.Messages;
using FluentAI.ChatCompletions.Abstraction.Tools;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using NJsonSchema.Generation;
using NSubstitute;

namespace Tests;

[TestFixture]
public class ChatCompletionTests
{
    private JsonSchema _jsonSchema;

    [SetUp]
    public async Task SetUp()
    {
        _jsonSchema = JsonSchema.FromType<TestJsonObject>(new SystemTextJsonSchemaGeneratorSettings()
        {
            SerializerOptions = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            }
        });
    }

    [Test]
    public async Task ReturnsChatCompletionMessageContentAsExpected()
    {
        var client = Substitute.For<IChatCompletionsClient>();
        var executor = new ChatCompletionsExecutor(client);

        client.GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>())
            .Returns(new ChatCompletionsResponse(
                new ChatCompletionsAssistantMessage("Test", []),
                false));

        var completion = await executor.GetChatCompletionsAsync(
            new ChatCompletionsOptions(),
            new Dictionary<string, IChatCompletionsTool>());

        Assert.That(completion.IsChatToolCall, Is.EqualTo(false));
        Assert.That(completion.CompletionMessage.Content, Is.EqualTo("Test"));
    }

    [Test]
    public async Task ParsesValidJsonChatCompletionResponse()
    {
        var client = Substitute.For<IChatCompletionsClient>();
        var executor = new ChatCompletionsExecutor(client);

        var validJson = "{'message': 'Test'}";

        client.GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>())
            .Returns(new ChatCompletionsResponse(
                new ChatCompletionsAssistantMessage(validJson, []),
                false));

        var completion = await executor.GetStructuredChatCompletionsAsync(
            new ChatCompletionsOptions(),
            _jsonSchema,
            new Dictionary<string, IChatCompletionsTool>());

        Assert.That(completion, Is.EqualTo(JObject.Parse(validJson)));
    }

    [Test]
    public async Task ThrowsExceptionForInvalidJsonStructure()
    {
        var client = Substitute.For<IChatCompletionsClient>();
        var executor = new ChatCompletionsExecutor(client);

        var invalidJson = "{'msg': 'Test'}";

        client.GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>())
            .Returns(new ChatCompletionsResponse(
                new ChatCompletionsAssistantMessage(invalidJson, []),
                false));

        Assert.ThrowsAsync<InvalidOperationException>(() => executor.GetStructuredChatCompletionsAsync(
            new ChatCompletionsOptions(),
            _jsonSchema,
            new Dictionary<string, IChatCompletionsTool>()));

        await client.Received(3).GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>());
    }

    [Test]
    public async Task ThrowsExceptionForMalformedJson()
    {
        var client = Substitute.For<IChatCompletionsClient>();
        var executor = new ChatCompletionsExecutor(client);

        var invalidJson = "{ Malformed Json }";

        client.GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>())
            .Returns(new ChatCompletionsResponse(
                new ChatCompletionsAssistantMessage(invalidJson, []),
                false));

        Assert.ThrowsAsync<InvalidOperationException>(() => executor.GetStructuredChatCompletionsAsync(
            new ChatCompletionsOptions(),
            _jsonSchema,
            new Dictionary<string, IChatCompletionsTool>()));

        await client.Received(3).GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>());
    }

    [Test]
    public async Task RetriesAndReturnsValidJsonAfterInitialFailure()
    {
        var client = Substitute.For<IChatCompletionsClient>();
        var executor = new ChatCompletionsExecutor(client);

        var invalidJson = "{ Malformed Json }";
        var validJson = "{'message': 'Test'}";

        client.GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>())
            .Returns(new ChatCompletionsResponse(
                new ChatCompletionsAssistantMessage(invalidJson, []),
                false),
                new ChatCompletionsResponse(
                    new ChatCompletionsAssistantMessage(validJson, []),
                    false));

        var completion = await executor.GetStructuredChatCompletionsAsync(
            new ChatCompletionsOptions(),
            _jsonSchema,
            new Dictionary<string, IChatCompletionsTool>());

        Assert.That(completion, Is.EqualTo(JObject.Parse(validJson)));
        await client.Received(2).GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>());
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    public async Task RetriesSpecifiedNumberOfTimesForInvalidJson(int retry)
    {
        var client = Substitute.For<IChatCompletionsClient>();
        var executor = new ChatCompletionsExecutor(client);

        var invalidJson = "{ Malformed Json }";

        client.GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>())
            .Returns(new ChatCompletionsResponse(
                    new ChatCompletionsAssistantMessage(invalidJson, []),
                    false));

        Assert.ThrowsAsync<InvalidOperationException>(() => executor.GetStructuredChatCompletionsAsync(
            new ChatCompletionsOptions(),
            _jsonSchema,
            new Dictionary<string, IChatCompletionsTool>(),
            maxRetries: retry));

        await client.Received(retry + 1).GetChatCompletionsAsync(Arg.Any<ChatCompletionsOptions>());
    }
}