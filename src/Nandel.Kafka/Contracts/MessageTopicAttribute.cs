using System;

namespace Nandel.Kafka.Contracts;

public class MessageTopicAttribute : Attribute
{
    public string TopicName { get; }

    public MessageTopicAttribute(string topicName)
    {
        TopicName = topicName;
    }
}