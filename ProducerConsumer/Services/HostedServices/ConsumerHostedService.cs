using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ProducerConsumer.Services.HostedServices;

public class ConsumerHostedService<TRequest, TResponse> : BackgroundService
{
    private readonly ConsumerService<TRequest, TResponse> _consumerService;

    public ConsumerHostedService(ConsumerService<TRequest, TResponse> consumerService)
    {
        _consumerService = consumerService;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return _consumerService.ConsumeMessagesAsync(stoppingToken);
    }
}
