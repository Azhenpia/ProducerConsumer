using ProducerConsumer.Services.Factories;
using ProducerConsumer.Services.HostedServices;

namespace ProducerConsumer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug).AddConsole(); 
        });

        var factory = new ProducerConsumerFactory(loggerFactory);

        var (producer, consumer) = factory.CreateProducerConsumerPair<string, string>(async (request) =>
        {
            await Task.Delay(new Random().Next(3000, 5000)); // Simulate processing delay
            return $"Processed {request}";
        });

        services.AddSingleton(producer);
        services.AddSingleton(consumer);

        // Add hosted service for getting message from channel
        services.AddHostedService<ConsumerHostedService<string, string>>();
        services.AddHostedService<ProducerHostedService<string, string>>();

        services.AddControllers();
        services.AddSwaggerGen();
        services.AddLogging(builder => builder.AddConsole());
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}


