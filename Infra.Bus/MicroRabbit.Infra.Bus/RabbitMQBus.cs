using System.Text;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.Commands;
using MicroRabbit.Domain.Core.Events;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MicroRabbit.Infra.Bus;

public sealed class RabbitMQBus : IEventBus
{
    private readonly IMediator _mediator;
    private readonly Dictionary<string, List<Type>> _handlers;
    private readonly List<Type> _eventTypes;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RabbitMQBus(IMediator mediator, IServiceScopeFactory serviceScopeFactory)
    {
        _mediator = mediator;
        _serviceScopeFactory = serviceScopeFactory;
        _handlers = new Dictionary<string, List<Type>>();
        _eventTypes = new List<Type>();
    }

    public Task SendCommand<T>(T command) where T : Command
    {
        return _mediator.Send(command);
    }

    public async Task Publish<T>(T @event) where T : Event
    {
        try
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            var eventName = @event.GetType().Name;

            await channel.QueueDeclareAsync(eventName, false, false, false, null);

            var message = JsonConvert.SerializeObject(@event);
            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: eventName,
                mandatory: true,
                basicProperties: new BasicProperties
                {
                    ContentType = "text/plain",
                    DeliveryMode = DeliveryModes.Persistent
                },
                body: body,
                cancellationToken: default
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to publish event {@event.GetType().Name}", ex);
        }
    }

    public void Subscribe<T, EH>()
        where T : Event
        where EH : IEventHandler<T>
    {
        var eventName = typeof(T).Name;
        var handlerType = typeof(EH);

        if (!_eventTypes.Contains(typeof(T)))
        {
            _eventTypes.Add(typeof(T));
        }

        if (!_handlers.ContainsKey(eventName))
        {
            _handlers.Add(eventName, new List<Type>());
        }

        if (_handlers[eventName].Any(h => h.GetType() == handlerType))
        {
            throw new ArgumentException($"The handler type {handlerType.Name} already is registered for '{nameof(handlerType)}'");
        }

        _handlers[eventName].Add(handlerType);

        StartBasicConsume<T>();
    }

    private async void StartBasicConsume<T>() where T : Event
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };

        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();

        var eventName = typeof(T).Name;

        await channel.QueueDeclareAsync(eventName, false, false, false, null);

        var consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += Consumer_Received;

        await channel.BasicConsumeAsync(eventName, true, consumer);
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs e)
    {
        var eventName = e.RoutingKey;
        var message = Encoding.UTF8.GetString(e.Body.Span);

        try
        {
            await ProcessEvent(eventName, message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    private async Task ProcessEvent(string eventName, string message)
    {
        if (!_handlers.ContainsKey(eventName))
        {
            return;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var subscriptions = _handlers[eventName];

        foreach (var subscription in subscriptions)
        {
            var handler = scope.ServiceProvider.GetService(subscription);
            if (handler is null)
            {
                continue;
            }

            var eventType = _eventTypes.SingleOrDefault(et => et.Name == eventName);
            if (eventType is null)
            {
                throw new InvalidOperationException($"Event type '{eventName}' not found in registered event types.");
            }

            var @event = JsonConvert.DeserializeObject(message, eventType);
            if (@event is null)
            {
                throw new InvalidOperationException($"Failed to deserialize message to event type '{eventType.Name}'.");
            }

            var concreteType = typeof(IEventHandler<>).MakeGenericType(eventType);
            if (concreteType is null)
            {
                throw new InvalidOperationException($"Handler type for event '{eventName}' not found.");
            }

            var handlerMethod = concreteType.GetMethod("Handle");
            if (handlerMethod is null)
            {
                throw new InvalidOperationException($"Handle method not found for handler type '{concreteType.Name}'.");
            }

            if (!handlerMethod.IsPublic || handlerMethod.ReturnType != typeof(Task))
            {
                throw new InvalidOperationException($"Handle method must be public and return Task in handler type '{concreteType.Name}'.");
            }

            await (Task)handlerMethod.Invoke(handler, new object[] { @event });
        }
    }
}
