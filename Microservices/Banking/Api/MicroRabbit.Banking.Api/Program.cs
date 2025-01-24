using Microsoft.EntityFrameworkCore;
using MicroRabbit.Banking.Data.Context;
using MicroRabbit.Infra.IoC;
using MediatR;
using System.Reflection;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();

builder.Services.AddDbContext<BankingDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BankingDbConnection")
               .Replace("$BANKING_DB_PASSWORD", Environment.GetEnvironmentVariable("BANKING_DB_PASSWORD"))
    )
);

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(Assembly.GetExecutingAssembly()));
builder.Services.AddControllers();
builder.Services.AddMvc();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "MicroRabbit.Banking.API", Version = "v1" });
});

DependencyContainer.RegisterServices(builder.Services);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MicroRabbit.Banking.API v1"));

app.Run();
