using MediatR;
using MicroRabbit.Banking.Application.Interfaces;
using MicroRabbit.Banking.Application.Services;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Banking.Data.Repository;
using MicroRabbit.Banking.Domain.Commands;
using MicroRabbit.Banking.Domain.Interfaces;
using MicroRabbit.Domain.Core.Bus;
using MicroRabbit.Domain.Core.CommandHandlers;
using MicroRabbit.Infra.Bus;
using MicroRabbit.Transfer.Application.Interfaces;
using MicroRabbit.Transfer.Application.Services;
using MicroRabbit.Transfer.Data.Context;
using MicroRabbit.Transfer.Data.Repository;
using MicroRabbit.Transfer.Domain.EventHandlers;
using MicroRabbit.Transfer.Domain.Events;
using MicroRabbit.Transfer.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MicroRabbit.Infra.IoC;

public class DependencyContainer
{
    public static void RegisterServices(IServiceCollection services)
    {
        // Domain bus
        services.AddSingleton<IEventBus, RabbitMQBus>(serviceProvider =>
        {
            var scopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMQBus(serviceProvider.GetService<IMediator>(), scopeFactory);
        });

        // Domain events
        services.AddTransient<TransferEventHandler>();
        services.AddTransient<IEventHandler<TransferCreatedEvent>, TransferEventHandler>();

        // Application services
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<ITransferService, TransferService>();

        services.AddDbContext<BankingDbContext>(options =>
            options.UseSqlServer(
                services.BuildServiceProvider().GetRequiredService<IConfiguration>()
                    .GetConnectionString("BankingDbConnection")
                    .Replace("$BANKING_DB_PASSWORD", Environment.GetEnvironmentVariable("BANKING_DB_PASSWORD"))
            )
        );

        services.AddDbContext<TransferDbContext>(options =>
            options.UseSqlServer(
                services.BuildServiceProvider().GetRequiredService<IConfiguration>()
                    .GetConnectionString("TransferDbConnection")
                    .Replace("$BANKING_DB_PASSWORD", Environment.GetEnvironmentVariable("BANKING_DB_PASSWORD"))
            )
        );

        // Data layer repositories
        services.AddTransient<IAccountRepository, AccountRepository>();
        services.AddTransient<ITransferRepository, TransferRepository>();

        // Domain Banking Commands
        services.AddTransient<IRequestHandler<CreateTransferCommand, bool>, TransferCommandHandler>();
    }
}

