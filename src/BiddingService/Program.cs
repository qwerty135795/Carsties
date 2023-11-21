using BiddingService;
using BiddingService.Consumers;
using BiddingService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;
using MongoDB.Entities;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddControllers();
builder.Services.AddMassTransit(c => {
    
    c.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();
    c.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("bids", false));
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
builder.Services.AddScoped<GrpcAuctionClient>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["IdentityServiceUrl"];
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.NameClaimType = "username";
    });
builder.Services.AddHostedService<CheckAuctionFinished>();
var app = builder.Build();


app.UseAuthorization();

app.MapControllers();
await DB.InitAsync("BidDb", MongoClientSettings.FromConnectionString(
    builder.Configuration.GetConnectionString("BidDb")));
app.Run();
