using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Domain.Interfaces;
using MicroRabbit.Transfer.Domain.Models;

namespace MicroRabbit.Transfer.Data.Repository;

public class TransferRepository : ITransferRepository
{
    private readonly TransferDbContext _ctx;

    public TransferRepository(TransferDbContext ctx)
    {
        _ctx = ctx;
    }

    public void Add(TransferLog transferLog)
    {
        _ctx.Add(transferLog);
        _ctx.SaveChanges();
    }

    public IEnumerable<TransferLog> GetTransferLogs()
    {
        return _ctx.TransferLogs;
    }
}
