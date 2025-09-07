using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;

namespace Nandel.Kafka.ErrorHandling;

public class KafkaErrorHandler : IKafkaErrorHandler
{
    private readonly IAdminClient _admin;
    private readonly ILogger<KafkaErrorHandler> _logger;

    public KafkaErrorHandler(IAdminClient admin, ILogger<KafkaErrorHandler> logger)
    {
        _admin = admin;
        _logger = logger;
    }

    public bool CanHandle(KafkaException e)
    {
        return TryMatchHandler(e) is not null;
    }

    public async Task HandleAsync(KafkaException e)
    {
        var handleFunc = TryMatchHandler(e) ?? throw new InvalidOperationException("Can't handle provided exception", e);
        await handleFunc();
    }

    private Func<Task>? TryMatchHandler(KafkaException e)
    {
        return e switch
        {
            ConsumeException cex when cex.Error.Code == ErrorCode.UnknownTopicOrPart => () => UnknownTopicOrPartAsync(e.Error, cex.ConsumerRecord.Topic),
            ProduceException<string, string> pex when pex.Error.Code == ErrorCode.UnknownTopicOrPart => () => UnknownTopicOrPartAsync(e.Error, pex.DeliveryResult.Topic),
            
            // Default
            _ => null
        };
    }
    
    private async Task UnknownTopicOrPartAsync(Error error, string topic)
    {
        if (error.Code != ErrorCode.UnknownTopicOrPart) throw new InvalidOperationException("Wrong error code");
        
        try
        {
            var topicSpec = new TopicSpecification()
            {
                Name = topic,
                NumPartitions = 1,
            };

            await _admin.CreateTopicsAsync([topicSpec]);
            
            _logger.LogInformation(
                "\ud83c\udd95 Topic created {Topic} while handling the error {Error}.", 
                topic, error.Reason);
        }
        catch (CreateTopicsException ex) when (ex.Error.Code == ErrorCode.Local_Partial)
        {
            _logger.LogWarning(
                "\u2757 Attempted to create the topic {Topic}, which already exists, while handling the error {Error}.", 
                topic, error.Reason);
        }
    }
}