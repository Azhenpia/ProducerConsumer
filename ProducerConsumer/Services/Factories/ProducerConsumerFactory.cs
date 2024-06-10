using System.Threading.Channels;

namespace ProducerConsumer.Services.Factories;

public class ProducerConsumerFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ProducerConsumerFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public (ProducerService<TRequest, TResponse>, ConsumerService<TRequest, TResponse>) CreateProducerConsumerPair<TRequest, TResponse>(
        Func<TRequest, Task<TResponse>> processRequestAsync)
    {
        var requestChannel = Channel.CreateUnbounded<(Guid, TRequest)>();
        var responseChannel = Channel.CreateUnbounded< (Guid, TResponse)> ();

        var producerLogger = _loggerFactory.CreateLogger<ProducerService<TRequest, TResponse>>();
        var consumerLogger = _loggerFactory.CreateLogger<ConsumerService<TRequest, TResponse>>();

        var producer = new ProducerService<TRequest, TResponse>(requestChannel, responseChannel, producerLogger);
        var consumer = new ConsumerService<TRequest, TResponse>(requestChannel, responseChannel, processRequestAsync, consumerLogger);

        return (producer, consumer);
    }
}

public class ProducerConsumerPair<TRequest, TResponse>
{
    public ProducerService<TRequest, TResponse> Producer { get; }
    public ConsumerService<TRequest, TResponse> Consumer { get; }

    public ProducerConsumerPair(
        ProducerService<TRequest, TResponse> producer,
        ConsumerService<TRequest, TResponse> consumer)
    {
        Producer = producer;
        Consumer = consumer;
    }
}


