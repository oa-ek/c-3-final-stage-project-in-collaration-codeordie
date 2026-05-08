using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TravelManager.Domain.Entities;
using TravelManager.Infrastructure;
using TravelManager.Infrastructure.Data;
using TravelManager.Infrastructure.Interfaces;
using TravelManager.Infrastructure.Interfaces.IServices;
using TravelManager.Infrastructure.Repositories;
using TravelManager.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// ВИПРАВЛЕНО: прибрано зайвий перший виклик AddControllersWithViews()
builder.Services.AddControllersWithViews()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TravelManager API",
        Version = "v1",
        Description = "Публічний Web API для управління поїздками у TravelManager"
    });

    // Підключити XML-коментарі (/// <summary>) — необов'язково але красиво
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options => {
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<UkrainianIdentityErrorDescriber>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

builder.Services.AddTransient<IEmailService, EmailService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IDestinationInfoService, DestinationInfoService>();

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler(); 

builder.Services.AddHttpClient<ICountryInfoService, CountryInfoService>(client =>
{
    client.BaseAddress = new Uri("https://restcountries.com/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler();

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri("https://bank.gov.ua/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler();

builder.Services.AddHttpClient<INominatimService, NominatimService>(client => {
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0 (university project)");
}).AddStandardResilienceHandler();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "TravelManager API v1");
    c.RoutePrefix = "swagger"; // Swagger буде на /swagger
});

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAdminAsync(scope.ServiceProvider);
}

app.Run();