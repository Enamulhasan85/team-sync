using System;

namespace Template.Application.Common.Interfaces
{
    /// <summary>
    /// Abstraction for publishing integration events to message brokers.
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes a message of type T to the specified topic or routing key.
        /// </summary>
        Task PublishAsync<T>(string routingKey, T message);
    }
}
