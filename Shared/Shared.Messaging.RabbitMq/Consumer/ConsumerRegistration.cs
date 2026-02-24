using Shared.Messaging.RabbitMq.Abstractions;

namespace Shared.Messaging.RabbitMq.Consumer;

/// <summary>
/// Registration of a consumer for a specific message type on a specific queue.
/// Used by the RabbitMqConsumerHost to route messages.
/// </summary>
public sealed class ConsumerRegistration
{
    public string QueueName { get; }
    public Type MessageType { get; }
    public Type ConsumerType { get; }

    private ConsumerRegistration(string queueName, Type messageType, Type consumerType)
    {
        QueueName = queueName;
        MessageType = messageType;
        ConsumerType = consumerType;
    }

    public static ConsumerRegistration Create<TMessage, TConsumer>(string queueName)
        where TMessage : class
        where TConsumer : class, IMessageConsumer<TMessage>
    {
        return new ConsumerRegistration(queueName, typeof(TMessage), typeof(TConsumer));
    }

    public async Task InvokeConsumerAsync(
        IServiceProvider serviceProvider,
        object message,
        MessageContext context,
        CancellationToken cancellationToken)
    {
        var consumer = serviceProvider.GetService(ConsumerType)
            ?? throw new InvalidOperationException(
                $"Consumer {ConsumerType.Name} is not registered in DI");

        // Use reflection to call ConsumeAsync with the correct type
        var method = ConsumerType.GetMethod(nameof(IMessageConsumer<object>.ConsumeAsync))
            ?? throw new InvalidOperationException(
                $"Consumer {ConsumerType.Name} does not implement ConsumeAsync");

        var task = (Task)method.Invoke(consumer, [message, context, cancellationToken])!;
        await task;
    }
}
