using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using Nandel.Kafka.Contracts;
using Nandel.Kafka.ErrorHandling;

namespace Nandel.Kafka.Kafka;

public class KafkaMessagePublisher : IMessagePublisher
{
    private readonly IKafkaErrorHandler _errorHandler;
    private readonly IProducer<string, string> _producer;
    
    public KafkaMessagePublisher(IOptions<KafkaSettings> options, IKafkaErrorHandler errorHandler)
    {
        var config = new ProducerConfig().SetupConnection(options.Value);
        _producer = new ProducerBuilder<string, string>(config).Build(); 
        _errorHandler = errorHandler;
    }
    
    public async Task PublishAsync(string topic, string key, string value, CancellationToken cancel)
    {
        try
        {
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value }, cancel);
        }
        catch (KafkaException e) when (_errorHandler.CanHandle(e))
        {
            await _errorHandler.HandleAsync(e);
            await _producer.ProduceAsync(topic, new Message<string, string> { Key = key, Value = value }, cancel);
        }
    }
}