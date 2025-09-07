using System.Threading.Tasks;
using Confluent.Kafka;

namespace Nandel.Kafka.ErrorHandling;

public interface IKafkaErrorHandler
{
    bool CanHandle(KafkaException e);
    Task HandleAsync(KafkaException e);
}