using System;
using System.Linq;
using Confluent.Kafka;

namespace Nandel.Kafka.Kafka;

public static class ConfigHelper
{
    public static T SetupConnection<T>(this T config, KafkaSettings settings)
        where T : ClientConfig
    {
        // ✅ Validation
        if (!settings.Brokers.Any()) throw new InvalidOperationException("Kafka Settings must contain at least one broker.");
        
        // 🔌 Connection 
        config.BootstrapServers = string.Join(',', settings.Brokers);

        // 🔒 Security
        if (!string.IsNullOrEmpty(settings.SaslUsername))
        {
            config.SecurityProtocol = SecurityProtocol.SaslSsl;
            config.SaslMechanism = SaslMechanism.Plain;
            config.SaslUsername = settings.SaslUsername;
            config.SaslPassword = settings.SaslPassword;
        }

        return config;
    }
}