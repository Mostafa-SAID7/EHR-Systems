using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.EventBus.Broker;

/// <summary>
/// Interface for message broker abstraction (RabbitMQ, Kafka, Azure Service Bus).
/// Single responsibility: Broker connection and lifecycle management.
/// </summary>
public interface IMessageBroker : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Connect to message broker.
    /// </summary>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnect from message broker.
    /// </summary>
    Task DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if broker is connected.
    /// </summary>
    Task<bool> IsConnectedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get broker type/name.
    /// </summary>
    string BrokerType { get; }

    /// <summary>
    /// Publish message to topic/queue.
    /// </summary>
    Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Subscribe to topic/queue.
    /// </summary>
    Task SubscribeAsync(string topic, Func<string, Task> handler, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get broker health status.
    /// </summary>
    Task<BrokerHealthStatus> GetHealthAsync(CancellationToken cancellationToken = default);
}
