using ECommercePlatform.Application;
using ECommercePlatform.Infrastructure;
using ECommercePlatform.WebAPI;
using ECommercePlatform.WebAPI.Modules;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddRateLimiter(options =>
{
    // Standart Politika (Saniyede 3 istek)
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromSeconds(10);
        opt.QueueLimit = 0;
    });

    // Sýký Politika (Dakikada 3 istek)
    options.AddFixedWindowLimiter("strict", opt =>
    {
        opt.PermitLimit = 3;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
});
builder.Services.AddCors();
builder.Services.AddOpenApi();

//========================================================================================//
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseCors(x => x
.AllowAnyHeader()
.AllowAnyOrigin()
.AllowAnyMethod()
.SetPreflightMaxAge(TimeSpan.FromMinutes(10)));
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
//Endpoint Mapping
app.MapAuthEndpoints();
app.MapCompanyEndpoints();
await app.CreateFirstUser();
app.Run();
