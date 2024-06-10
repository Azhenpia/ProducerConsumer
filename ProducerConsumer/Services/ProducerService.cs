using System.Collections.Concurrent;
using System.Threading.Channels;

namespace ProducerConsumer.Services;

public class ProducerService<TRequest, TResponse>
{
    private readonly ChannelWriter<(Guid, TRequest)> _requestWriter;
    private readonly ChannelReader<(Guid, TResponse)> _responseReader;
    private readonly ConcurrentDictionary<Guid, TaskCompletionSource<TResponse>> _pendingRequests;
    private readonly ILogger<ProducerService<TRequest, TResponse>> _logger;

    public ProducerService(Channel<(Guid, TRequest)> requestChannel, Channel<(Guid, TResponse)> responseChannel, ILogger<ProducerService<TRequest, TResponse>> logger)
    {
        _requestWriter = requestChannel.Writer;
        _responseReader = responseChannel.Reader;
        _pendingRequests = new();
        _logger = logger;
    }

    public async Task<TResponse> PushMessageAsync(TRequest message)
    {
        Guid requestId = Guid.NewGuid();
        var completionSource = new TaskCompletionSource<TResponse>();
        _pendingRequests.TryAdd(requestId, completionSource);

        try
        {
            await _requestWriter.WriteAsync((requestId, message));
            _logger.LogInformation("Produced: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write message to the request channel");
            _pendingRequests.TryRemove(requestId, out _);
            completionSource.TrySetException(ex);
            throw;
        }

        return await completionSource.Task;
    }

    public async Task GetResponsesAsync(CancellationToken cancellationToken)
    {
        await foreach (var (id, response) in _responseReader.ReadAllAsync())
        {
            if (_pendingRequests.TryGetValue(id, out var completionSource))
            {
                _logger.LogInformation("Received response: {Response}", response);
                completionSource.TrySetResult(response);
                _pendingRequests.Remove(id, out _);
            }
            else
            {
                _logger.LogWarning("Invalid Request Id: {id}", id);
            }            
        }
    }
}
