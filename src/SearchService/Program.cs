using System.Net;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService.Consumers;
using SearchService.Data;
using SearchService.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddMassTransit(c =>
{
    c.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));
    c.UsingRabbitMq((context, cnf) =>
    {
        cnf.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cnf.ReceiveEndpoint("search-auction-created", cnf =>
        {
            cnf.UseMessageRetry(c => c.Interval(5, 5));
            cnf.ConfigureConsumer<AuctionCreatedConsumer>(context);
        });
        cnf.ConfigureEndpoints(context);
    });
});

var app = builder.Build();



app.UseAuthorization();

app.MapControllers();
app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception e)
    {
        Console.WriteLine(e.Message);
    }
});

app.Run();


static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions.HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(5));