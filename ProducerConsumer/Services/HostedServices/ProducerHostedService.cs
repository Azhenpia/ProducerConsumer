namespace ProducerConsumer.Services.HostedServices;

public class ProducerHostedService<TRequest, TResponse> : BackgroundService
{
    private readonly ProducerService<TRequest, TResponse> _producerService;

    public ProducerHostedService(ProducerService<TRequest, TResponse> producerService)
    {
        _producerService = producerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _producerService.GetResponsesAsync(stoppingToken);
    }
}

