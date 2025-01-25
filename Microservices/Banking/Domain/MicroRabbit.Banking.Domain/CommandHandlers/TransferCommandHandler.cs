using MicroRabbit.Banking.Domain.Commands;
using MediatR;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Banking.Domain.Events;

namespace MicroRabbit.Domain.Core.CommandHandlers;

public class TransferCommandHandler : IRequestHandler<CreateTransferCommand, bool>
{
    private readonly IEventBus _bus;

    public TransferCommandHandler(IEventBus bus)
    {
        _bus = bus;
    }

    public Task<bool> Handle(CreateTransferCommand request, CancellationToken cancellationToken)
    {
        _bus.Publish(new TransferCreatedEvent(request.From, request.To, request.Amount));

        return Task.FromResult(true);
    }
}
