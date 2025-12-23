using ECommercePlatform.MvcAdmin.Services;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<IApiService, ApiService>(opt =>
{
    opt.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
});
//redis & session
//builder.Services.AddStackExchangeRedisCache(opt =>
//{
//    opt.Configuration = builder.Configuration.GetConnectionString("Redis");
//    opt.InstanceName = "EComAdmin_";
//});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;
    opt.Cookie.IsEssential = true;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opt.Cookie.SameSite = SameSiteMode.Lax;
    opt.Cookie.Name = ".ECommerce.Admin.Session";
});

builder.Services.AddMapster();
//===============================================================//
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
