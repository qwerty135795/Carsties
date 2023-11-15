using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(opt =>
{
    opt.Authority = builder.Configuration["IdentityServiceUrl"];
    opt.TokenValidationParameters.ValidateAudience = false;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters.NameClaimType = "username";
});
var app = builder.Build();

app.MapReverseProxy();
app.UseAuthentication();
app.UseAuthorization();
app.Run();
