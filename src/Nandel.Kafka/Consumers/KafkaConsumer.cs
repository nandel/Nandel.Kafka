using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nandel.Kafka.Contracts;
using Nandel.Kafka.ErrorHandling;
using Nandel.Kafka.Kafka;

namespace Nandel.Kafka.Consumers;

public class KafkaConsumer<TMessage, THandler> : BackgroundService 
    where THandler : IMessageHandler<TMessage>
{
    private const int DEFAULT_WORKER_COUNT = 6;
    
    private readonly IKafkaErrorHandler _errorHandler;
    private readonly ILogger<KafkaConsumer<TMessage, THandler>> _logger;
    private readonly MessageConsumerAttribute _attributes;
    private readonly IConsumer<string, string> _consumer;
    private readonly KafkaConsumerWorker<TMessage, THandler>[] _workers;
    
    public KafkaConsumer(IKafkaErrorHandler errorHandler, ILogger<KafkaConsumer<TMessage, THandler>> logger, IOptions<KafkaSettings> options, IServiceProvider services)
    {
        _errorHandler = errorHandler;
        _logger = logger;
        
        _attributes = MessageConsumerAttribute.From(typeof(THandler)) 
            ?? throw new InvalidOperationException("Consumer is not decorated with MessageConsumerAttribute");

        _consumer = CreateConsumer(_attributes, options.Value);
        _workers = Enumerable.Range(0, DEFAULT_WORKER_COUNT)
            .Select(_ => ActivatorUtilities.CreateInstance<KafkaConsumerWorker<TMessage, THandler>>(services))
            .ToArray();
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // 💡 Let host continue startup
        
        _logger.LogInformation(
            "\ud83d\ude80 Starting Kafka Consumer {GroupId} connected at topic {Topic}",
            _attributes.GroupId, _attributes.TopicName);

        _consumer.Subscribe(_attributes.TopicName);

        foreach (var worker in _workers)
        {
            worker.Start(_consumer, _attributes, stoppingToken);
        }
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAndEnqueueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("\ud83d\udca4 Stopping Kafka Consumer {GroupId}", _attributes.GroupId);
            }
            catch (KafkaException e) when (_errorHandler.CanHandle(e))
            {
                await _errorHandler.HandleAsync(e);
            }
            catch (Exception e)
            {
                _logger.LogCritical(
                    e, "\ud83d\udea8 Unhandled exception at Kafka Consumer {GroupId}", 
                    _attributes.GroupId);
                
                await Task.Delay(1000, stoppingToken);
            }
        }
        
        _consumer.Close();
        
        _logger.LogInformation("\ud83d\udca4 Stopped Kafka Consumer {GroupId}", _attributes.GroupId);
    }

    private async Task ConsumeAndEnqueueAsync(CancellationToken cancel)
    {
        var consumeResult = _consumer.Consume(cancel);
        var envelope = new KafkaMessageEnvelope<TMessage>(consumeResult);
        
        // 💁 Is very important to understand that this is a virtual partitioning on each client
        // , kafka won't assign the same partition to multiple clients
        // , so on each client we can redo the partitions as long is consistent based on the message key
        var workerPartition = KafkaWorkerPartioner.GetPartition(envelope.Key, DEFAULT_WORKER_COUNT); 
        var worker = _workers[workerPartition];
        
        await worker.EnqueueAsync(envelope, cancel);
    }
    
    private static IConsumer<string, string> CreateConsumer(MessageConsumerAttribute attributes, KafkaSettings settings)
    {
        var consumerConfig = new ConsumerConfig()
        {
            GroupId = attributes.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
        };
        consumerConfig.SetupConnection(settings);
        return new ConsumerBuilder<string, string>(consumerConfig).Build();
    }
}