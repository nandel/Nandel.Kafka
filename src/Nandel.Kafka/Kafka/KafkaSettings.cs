using System.Collections.Generic;

namespace Nandel.Kafka.Kafka;

public class KafkaSettings
{
    public IEnumerable<string> Brokers { get; set; } = new List<string>();
    public string SaslUsername { get; set; } = string.Empty;
    public string SaslPassword { get; set; } = string.Empty;
}