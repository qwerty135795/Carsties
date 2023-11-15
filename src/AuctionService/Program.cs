using AuctionService.Consumers;
using AuctionService.Data;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
        cnf.Host(builder.Configuration["RabbitMq:Host"], "/", host =>
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });
        cnf.ConfigureEndpoints(context);
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.Authority = builder.Configuration["IdentityServiceUrl"];
        opt.RequireHttpsMetadata = false;
        opt.TokenValidationParameters.ValidateAudience = false;
        opt.TokenValidationParameters.NameClaimType = "username";
    });
var app = builder.Build();
app.UseAuthentication();
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
