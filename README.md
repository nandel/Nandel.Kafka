# Nandel.Kafka

A Kafka Publisher and Consumer Library

## How to Use

Consumers should implement the interface `IMessageHandler<TMessage>` where `TMessage` is a class that represents your 
**message body**.

```csharp
using Nandel.Kafka.Contracts;

[MessageTopic(TopicName)]
public class Message
{
    public const string TopicName = "benchs.throughput.message";

    [JsonPropertyName("value")] public Guid Value { get; set; }
}

[MessageConsumer(Message.TopicName, "benchs.throughput.consumer")]
public class Consumer(ILogger<Consumer> logger) : IMessageHandler<Message>
{
    public Task HandleAsync(IMessageEnvelope<Message> envelope, Message message, CancellationToken cancel)
    {
        logger.LogInformation("Message Consumed");
        return Task.CompletedTask;
    }
}
```

You can **publish** your messages too using the service `IMessagePublisher`, this interface is able to publish all messages 
types.

```csharp
public class Producer(IMessagePublisher publisher)
{
    public async Task PublishMessageAsync(CancellationToken cancel)
    {
        var message = new Message { Value = Guid.NewGuid() };
        await publisher.PublishAsync(message.Value.ToString(), message, cancel);
    }
}
```

Setting up your project to work with Nandel.Kafka you need first to install it through the command 
`dotnet add package Nandel.Kafka`, after that you will need register the library services in your dependency injection 
system using the new method `AddNandelKafka` available as an extension of `IServiceCollection`.

For each implementation of `IMessageHandler<T>` you need to setup a consumer to start adding it to your dependency 
injection container using the extension `.AddKafkaMessageConsumer<TMessage, TConsumer>()`.

```csharp
IServiceCollection services;
services.AddNandelKafka(builder.Configuration.GetSection("Kafka")); // this configures Nandel.Kafka
services.AddKafkaMessageConsumer<Message, Consumer>(); // this setups a consumer to start
```

## Benchmark Results

ThroughputBenchmark

```console
Execution Length (seconds): 300
Messages produced: 1599532
Messages consumed: 1599532
Messages per second: 4,669.84
:
OS: Microsoft Windows NT 10.0.26100.0; 64-bit:True
Number of logical processors: 8
.NET version: 9.0.6
```