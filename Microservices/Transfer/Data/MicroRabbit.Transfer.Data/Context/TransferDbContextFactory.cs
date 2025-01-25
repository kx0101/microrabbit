using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MicroRabbit.Transfer.Data.Context;

public class TransferDbContextFactory : IDesignTimeDbContextFactory<TransferDbContext>
{
    public TransferDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TransferDbContext>();

        var connectionString = configuration.GetConnectionString("TransferDbConnection");
        optionsBuilder.UseSqlServer(connectionString);

        return new TransferDbContext(optionsBuilder.Options);
    }
}
