using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nandel.Kafka.Contracts;
using Nandel.Kafka.Kafka;

namespace Nandel.Kafka.Consumers;

public class KafkaConsumerWorker<TMessage, THandler>
    where THandler : IMessageHandler<TMessage>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<KafkaConsumerWorker<TMessage, THandler>> _logger;
    
    private readonly Channel<KafkaMessageEnvelope<TMessage>> _channel = Channel.CreateBounded<KafkaMessageEnvelope<TMessage>>(new BoundedChannelOptions(capacity: 80)
    {
        FullMode = BoundedChannelFullMode.Wait 
    });
    
    private IConsumer<string, string>? _consumer;
    private MessageConsumerAttribute? _attributes;
    private Task? _processQueueTask;

    public KafkaConsumerWorker(IServiceScopeFactory scopeFactory, ILogger<KafkaConsumerWorker<TMessage, THandler>> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void Start(IConsumer<string, string> consumer, MessageConsumerAttribute attributes, CancellationToken stopToken)
    {
        if (_processQueueTask is not null) throw new InvalidOperationException("Already running");

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug(
                "\ud83d\ude80 Starting Kafka Consumer Worker for {GroupId}",
                attributes.GroupId);   
        }
        
        _consumer = consumer;
        _attributes = attributes;
        _processQueueTask = Task.Run(() => ProcessQueueAsync(stopToken), stopToken);
    }

    public async Task EnqueueAsync(KafkaMessageEnvelope<TMessage> message, CancellationToken cancel)
    {
        await _channel.Writer.WriteAsync(message, cancel);
    }

    private async Task ProcessQueueAsync(CancellationToken stopToken)
    {
        if (_consumer is null) throw new InvalidOperationException("Consumer is null");
        if (_attributes is null) throw new InvalidOperationException("Attributes is null");
        
        await foreach (var envelope in _channel.Reader.ReadAllAsync(stopToken))
        {
            try
            {
                await HandleAsync(envelope, stopToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "\ud83d\udea8 Kafka Consumer Worker Error for {GroupId}", _attributes.GroupId);
            }
        }
    }

    private async Task HandleAsync(KafkaMessageEnvelope<TMessage> envelope, CancellationToken cancel)
    {
        if (_consumer is null) throw new InvalidOperationException("Consumer is null");
        if (_attributes is null) throw new InvalidOperationException("Attributes is null");
        
        await using var scope = _scopeFactory.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<THandler>();
        var handled = false;

        while (!handled)
        {
            try
            {
                await handler.HandleAsync(envelope, envelope.Value, cancel);
                handled = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "\ud83d\udea8 Kafka Consumer {GroupId} Handler Error connected at topic {Topic} handling message Partition={Partition} Offset={Offset}.",
                    _attributes.GroupId, _attributes.TopicName, envelope.ConsumeResult.Partition, envelope.ConsumeResult.Offset);
            
                await Task.Delay(1000, cancel);
            }
        }
        
        _consumer.Commit(envelope.ConsumeResult); // 💡 Only commit when we succeeded
    }
}