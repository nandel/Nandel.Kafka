using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Nandel.Kafka.Consumers;
using Nandel.Kafka.Contracts;
using Nandel.Kafka.ErrorHandling;
using Nandel.Kafka.Kafka;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddNandelKafka(this IServiceCollection services, IConfigurationSection config)
    {
        services.AddOptions<KafkaSettings>().Bind(config);
        
        services.TryAddSingleton<IMessagePublisher, KafkaMessagePublisher>();
        services.TryAddSingleton<IKafkaErrorHandler, KafkaErrorHandler>();
        
        services.TryAddSingleton<IAdminClient>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<KafkaSettings>>();
            var clientConfig = new AdminClientConfig().SetupConnection(options.Value);
            return new AdminClientBuilder(clientConfig).Build();
        });
    }
    
    public static void AddMessageConsumer<TMessage, TConsumer>(this IServiceCollection services)
        where TConsumer : class, IMessageHandler<TMessage>
    {
        services.TryAddScoped<TConsumer>();
        services.AddHostedService<KafkaConsumer<TMessage, TConsumer>>();
    }
}