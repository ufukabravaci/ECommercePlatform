using ECommercePlatform.Application;
using ECommercePlatform.Infrastructure;
using ECommercePlatform.WebAPI;
using ECommercePlatform.WebAPI.Modules;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(cfr =>
{
    cfr.AddFixedWindowLimiter("fixed", options =>
 {
     options.PermitLimit = 5;
     options.QueueLimit = 2;
     options.Window = TimeSpan.FromSeconds(10);
     options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
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
app.RegisterAuthRoutes();
await app.CreateFirstUser();
app.Run();
