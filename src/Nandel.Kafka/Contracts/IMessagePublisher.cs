using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Nandel.Kafka.Contracts;

public interface IMessagePublisher
{
    private static readonly ConcurrentDictionary<Type, string> s_topics = new(); 
    
    Task PublishAsync(string topic, string key, string value, CancellationToken cancel);
    
    public async Task PublishAsync<T>(string key, T value, CancellationToken cancel)
    {
        // Yeah, it's a trait, deal with it!
        
        if (!s_topics.TryGetValue(typeof(T), out var topicName))
        {
            topicName = typeof(T).GetCustomAttributes(inherit: false).OfType<MessageTopicAttribute>().First().TopicName;
            s_topics.TryAdd(typeof(T), topicName);
        }

        var messageValue = JsonSerializer.Serialize(value);
        
        await PublishAsync(topicName, key, messageValue, cancel);
    }
}