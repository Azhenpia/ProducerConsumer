using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Moq;

namespace ProducerConsumer.Services.Tests
{
    public class ProducerServiceTests
    {
        private readonly Channel<(Guid, string)> _requestChannel;
        private readonly Channel<(Guid, string)> _responseChannel;
        private readonly Mock<ILogger<ProducerService<string, string>>> _loggerMock;
        private readonly Mock<Func<string, Task<string>>> _processMessageAsyncMock;

        public ProducerServiceTests()
        {
            _requestChannel = Channel.CreateUnbounded<(Guid, string)>();
            _responseChannel = Channel.CreateUnbounded<(Guid, string)>();
            _loggerMock = new Mock<ILogger<ProducerService<string, string>>>();
            _processMessageAsyncMock = new Mock<Func<string, Task<string>>>();
        }

        [Fact]
        public async Task PushMessageAsync_ShouldEnqueueMessage()
        {
            // Arrange
            var producerService = new ProducerService<string, string>(_requestChannel, _responseChannel, _loggerMock.Object);
            var message = "Test Message";

            // Act
            var task = producerService.PushMessageAsync(message);

            // Assert
            var result = await _requestChannel.Reader.ReadAsync();
            Assert.Equal(message, result.Item2);
        }

        [Fact]
        public async Task GetResponsesAsync_ShouldProcessResponse()
        {
            // Arrange
            var consumerLoggerMock = new Mock<ILogger<ConsumerService<string, string>>>();
            var producerService = new ProducerService<string, string>(_requestChannel, _responseChannel, _loggerMock.Object);
            var message = "Test Message";
            var response = "Test Response";

            var cts = new CancellationTokenSource();

            // Act
            var getResponsesTask = Task.Run(() => producerService.GetResponsesAsync(cts.Token), cts.Token);

            var pushTask = producerService.PushMessageAsync(message);
            var requestId = await _requestChannel.Reader.ReadAsync();
            await _responseChannel.Writer.WriteAsync((requestId.Item1, response));

            // Cancel the GetResponsesAsync task after the response is processed
            await Task.Delay(100); // Ensure the response is processed
            cts.Cancel();

            // Assert
            var result = await pushTask;
            Assert.Equal(response, result);
        }

        [Fact]
        public void PushMultipleMessagesAsync_EnqueuesAllMessages()
        {
            // Arrange
            var requestChannel = Channel.CreateUnbounded<(Guid, string)>();
            var responseChannel = Channel.CreateUnbounded<(Guid, string)>();
            var loggerMock = new Mock<ILogger<ProducerService<string, string>>>();

            var producerService = new ProducerService<string, string>(requestChannel, responseChannel, loggerMock.Object);

            var messages = new List<string> { "Message1", "Message2", "Message3" };

            // Act
            foreach (var message in messages)
            {
                _ = producerService.PushMessageAsync(message);
            }

            // Assert
            var enqueuedMessages = new List<string>();
            while (requestChannel.Reader.TryRead(out var message))
            {
                enqueuedMessages.Add(message.Item2);
            }

            Assert.Equal(messages.Count, enqueuedMessages.Count);
            Assert.Contains("Message1", enqueuedMessages);
            Assert.Contains("Message2", enqueuedMessages);
            Assert.Contains("Message3", enqueuedMessages);
        }
    }
}

