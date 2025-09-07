using System.Threading;
using System.Threading.Tasks;

namespace Nandel.Kafka.Contracts;

public interface IMessageHandler<T>
{
    Task HandleAsync(IMessageEnvelope<T> envelope, T message, CancellationToken cancel);
}