using NUnit.Framework;
using NSubstitute;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAI.ChatCompletions.Common.Clients;
using FluentAI.ChatCompletions.Common.Messages;
using FluentAI.ChatCompletions.Common.Tools;
using Newtonsoft.Json.Linq;
using NJsonSchema;
using System;
using FluentAI.ChatCompletions.Common;
using FluentAI.ChatCompletions.Tools;

namespace FluentAI.ChatCompletions.Tests
{
    [TestFixture]
    public class ChatCompletionClientBaseTests
    {
        private ChatCompletionClientBase _client;
        private ChatCompletionsOptions _options;
        private JsonSchema _schema;
        private IChatCompletionTool _tool;
        private IReadOnlyDictionary<string, IChatCompletionTool> _toolbox;

        [SetUp]
        public async Task SetUp()
        {
            _client = Substitute.ForPartsOf<ChatCompletionClientBase>();
            _options = new ChatCompletionsOptions
            {
                DeploymentName = "test-deployment",
                ResponseFormat = ChatCompletionFormat.Json
            };
            _schema = await JsonSchema.FromJsonAsync(@"{ 'type': 'object', 'properties': { 'valid': { 'type': 'string' } }, 'required': ['valid'] }");
            _tool = Substitute.For<IChatCompletionTool>();
            _toolbox = new Dictionary<string, IChatCompletionTool> { { "testTool", _tool } };
        }

        [Test]
        public async Task GetStructuredChatCompletionsAsync_ValidResponse_ReturnsJObject()
        {
            // Arrange
            var validJson = new JObject { ["valid"] = "json" };
            var validResponse = new ChatCompletionResponse
            {
                CompletionMessage = new ChatCompletionAssistantMessage(validJson.ToString(), []),
                IsChatToolCall = false
            };

            _client.GetChatCompletionsAsync(_options, _toolbox).Returns(Task.FromResult(validResponse));

            // Act
            var result = await _client.GetStructuredChatCompletionsAsync(_options, _schema, _toolbox);

            // Assert
            Assert.That(result, Is.EqualTo(validJson));
        }

        [Test]
        public void GetStructuredChatCompletionsAsync_InvalidJsonResponse_ThrowsException()
        {
            // Arrange
            var invalidJson = new JObject { ["invalid"] = "json" };
            var invalidResponse = new ChatCompletionResponse
            {
                CompletionMessage = new ChatCompletionAssistantMessage(invalidJson.ToString(), []),
                IsChatToolCall = false
            };

            _client.GetChatCompletionsAsync(_options, _toolbox).Returns(Task.FromResult(invalidResponse));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _client.GetStructuredChatCompletionsAsync(_options, _schema, _toolbox));
        }

        [Test]
        public void GetStructuredChatCompletionsAsync_InvalidResponse_ThrowsException()
        {
            // Arrange
            var invalidJson = @"Here's the json: {'invalid': 'json'}";
            var invalidResponse = new ChatCompletionResponse
            {
                CompletionMessage = new ChatCompletionAssistantMessage(invalidJson, []),
                IsChatToolCall = false
            };

            _client.GetChatCompletionsAsync(_options, _toolbox).Returns(Task.FromResult(invalidResponse));

            // Act & Assert
            Assert.ThrowsAsync<InvalidOperationException>(async () => await _client.GetStructuredChatCompletionsAsync(_options, _schema, _toolbox));
        }

        [Test]
        public async Task GetStructuredChatCompletionsAsync_RetriesOnFailure()
        {
            // Arrange
            var invalidJson = new JObject { ["invalid"] = "json" };
            var validJson = new JObject { ["valid"] = "json" };

            var invalidResponse = new ChatCompletionResponse
            {
                CompletionMessage = new ChatCompletionAssistantMessage(invalidJson.ToString(), []),
                IsChatToolCall = false
            };

            var validResponse = new ChatCompletionResponse
            {
                CompletionMessage = new ChatCompletionAssistantMessage(validJson.ToString(), []),
                IsChatToolCall = false
            };

            _client.GetChatCompletionsAsync(_options, _toolbox).Returns(
                Task.FromResult(invalidResponse),
                Task.FromResult(invalidResponse),
                Task.FromResult(validResponse)
            );

            // Act
            var result = await _client.GetStructuredChatCompletionsAsync(_options, _schema, _toolbox);

            // Assert
            Assert.AreEqual(validJson, result);
            await _client.Received(3).GetChatCompletionsAsync(_options, _toolbox);
        }

        // [Test]
        // public async Task GetChatCompletionsAsync_WithToolCalls_ReturnsFinalResponse()
        // {
        //     // Arrange
        //     var toolCall = new ChatCompletionsFunctionCall { Name = "testTool", Arguments = "{}" };
        //     var initialResponse = new ChatCompletionResponse
        //     {
        //         CompletionMessage = new ChatCompletionAssistantMessage("initial response", new List<ChatCompletionsFunctionCall> { toolCall }),
        //         IsChatToolCall = true
        //     };
        //
        //     var toolCallResponse = new ChatCompletionToolMessage("tool call response", "toolId");
        //     var finalResponse = new ChatCompletionResponse
        //     {
        //         CompletionMessage = new ChatCompletionAssistantMessage("final response", new List<ChatCompletionsFunctionCall>()),
        //         IsChatToolCall = false
        //     };
        //
        //     _client.GetChatCompletionsAsync(_options).Returns(
        //         Task.FromResult(initialResponse),
        //         Task.FromResult(finalResponse)
        //     );
        //
        //     _client.When(x => x.GetToolCallResponseMessage(Arg.Any<ChatCompletionsFunctionCall>(), _toolbox))
        //            .DoNotCallBase()
        //            .Returns(Task.FromResult(toolCallResponse));
        //
        //     // Act
        //     var result = await _client.GetChatCompletionsAsync(_options, _toolbox);
        //
        //     // Assert
        //     Assert.AreEqual(finalResponse, result);
        //     await _client.Received(2).GetChatCompletionsAsync(_options);
        // }
    }
}