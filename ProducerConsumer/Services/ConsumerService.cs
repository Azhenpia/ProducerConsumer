using System;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ProducerConsumer.Services;

public class ConsumerService<TRequest, TResponse>
{
    private readonly ChannelReader<(Guid, TRequest)> _requestReader;
    private readonly ChannelWriter<(Guid, TResponse)> _responseWriter;

    private readonly Func<TRequest, Task<TResponse>> ProcessMessageAsync;
    private readonly ILogger<ConsumerService<TRequest, TResponse>> _logger;

    public ConsumerService(
        Channel<(Guid, TRequest)> requestChannel,
        Channel<(Guid, TResponse)> responseChannel,
        Func<TRequest, Task<TResponse>> func,
        ILogger<ConsumerService<TRequest, TResponse>> logger)
    {
        _requestReader = requestChannel.Reader;
        _responseWriter = responseChannel.Writer;
        ProcessMessageAsync = func ?? throw new ArgumentNullException(nameof(ProcessMessageAsync), "No processing function defined.");
        _logger = logger;
    }

    public async Task ConsumeMessagesAsync(CancellationToken cancellationToken)
    {
        await foreach (var (id, message) in _requestReader.ReadAllAsync())
        {
            _logger.LogDebug("Number of message left in request queue: {count}", _requestReader.Count);
            // Process each message in a async call
            _ = ConsumeMessagesAsync(id, message);
        }
    }

    private async Task ConsumeMessagesAsync(Guid id, TRequest message)
    {
        try
        {
            _logger.LogInformation($"Consumed: {message}");

            var response = await ProcessMessageAsync(message);
            _logger.LogInformation($"Produced: {response}");
            await _responseWriter.WriteAsync((id, response));
        }
        catch (Exception ex)
        {
            _logger.LogError("Error processing message: {Message}", ex.Message);
        }
    }
}
