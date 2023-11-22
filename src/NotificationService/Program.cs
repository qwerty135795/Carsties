using MassTransit;
using NotificationService.Consumers;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMassTransit(c => {
    c.AddConsumersFromNamespaceContaining<BidPlacedConsumer>();
    c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("notify", false));
    c.UsingRabbitMq((context, cnf) =>
    {
        cnf.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cnf.ConfigureEndpoints(context);
    });
});
builder.Services.AddSignalR();
var app = builder.Build();

app.MapHub<NotificationHub>("/notify");
app.Run();
