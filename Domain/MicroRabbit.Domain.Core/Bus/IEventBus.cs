using MicroRabbit.Domain.Core.Events;
using MicroRabbit.Domain.Core.Commands;

namespace MicroRabbit.Domain.Core.Bus;

public interface IEventBus
{
    Task SendCommand<T>(T command) where T : Command;

    Task Publish<T>(T @event) where T : Event;

    void Subscribe<T, EH>()
        where T : Event
        where EH : IEventHandler<T>;
}
