using System;
using Microsoft.Extensions.Logging;
using Moq;
using ProducerConsumer.Services;
using System.Threading.Channels;

namespace ProducerConsumer.Tests
{
	public class ConsumerServiceTests
	{
        private readonly Channel<(Guid, string)> _requestChannel;
        private readonly Channel<(Guid, string)> _responseChannel;
        private readonly Mock<ILogger<ConsumerService<string, string>>> _loggerMock;

        public ConsumerServiceTests()
        {
            _requestChannel = Channel.CreateUnbounded<(Guid, string)>();
            _responseChannel = Channel.CreateUnbounded<(Guid, string)>();
            _loggerMock = new Mock<ILogger<ConsumerService<string, string>>>();
        }

        [Fact]
        public async Task ConsumeMessagesAsync_ShouldProcessMessage()
        {
            // Arrange
            Func<string, Task<string>> processMessageAsync = async message =>
            {
                await Task.Delay(100); // Simulate processing delay
                return message.ToUpper();
            };
            var consumerService = new ConsumerService<string, string>(_requestChannel, _responseChannel, processMessageAsync, _loggerMock.Object);
            var message = "Test Message";
            var requestId = Guid.NewGuid();
            await _requestChannel.Writer.WriteAsync((requestId, message));
            var cts = new CancellationTokenSource();

            // Act
            var consumeMessageTask = Task.Run(() => consumerService.ConsumeMessagesAsync(cts.Token), cts.Token);

            // Cancel the GetResponsesAsync task after the response is processed
            await Task.Delay(100); // Ensure the response is processed
            cts.Cancel();

            // Assert
            var result = await _responseChannel.Reader.ReadAsync();
            Assert.Equal(requestId, result.Item1);
            Assert.Equal("TEST MESSAGE", result.Item2);
        }
    }
}

