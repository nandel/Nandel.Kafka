using System;
using System.Linq;

namespace Nandel.Kafka.Contracts;

public class MessageConsumerAttribute : Attribute
{
    public string TopicName { get; }
    public string GroupId { get; }

    public MessageConsumerAttribute(string topicName, string groupId)
    {
        TopicName = topicName;
        GroupId = groupId;
    }

    public static MessageConsumerAttribute? From(Type type)
    {
        return type.GetCustomAttributes(inherit: false)
            .OfType<MessageConsumerAttribute>()
            .FirstOrDefault();
    }
}