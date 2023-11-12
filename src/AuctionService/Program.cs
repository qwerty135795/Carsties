using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddDbContext<AuctionDbContext>(opt =>
{
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddMassTransit(c => {
    c.AddEntityFrameworkOutbox<AuctionDbContext>(opt => 
    {
        opt.QueryDelay = TimeSpan.FromSeconds(10);

        opt.UsePostgres();
        opt.UseBusOutbox();
    });
    c.AddConsumersFromNamespaceContaining<AuctionCreatedFaultConsumer>();
    c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("auction", false));
    c.UsingRabbitMq((context, cnf) =>
    {
        cnf.ConfigureEndpoints(context);
    });
});
var app = builder.Build();

app.UseAuthorization();

app.MapControllers();
try 
{
    DbInitializer.InitDb(app);
}
catch(Exception e)
{
    Console.WriteLine(e.Message);
}
app.Run();
