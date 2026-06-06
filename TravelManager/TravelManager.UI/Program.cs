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

builder.Services.AddControllersWithViews()
.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TravelManager API",
        Version = "v1",
        Description = "Web API для TravelManager"
    });


var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath);


});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(connectionString));

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequireDigit = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddErrorDescriber<UkrainianIdentityErrorDescriber>();

builder.Services.AddAuthentication()
.AddGoogle(options =>
{
    IConfigurationSection googleAuthNSection =
    builder.Configuration.GetSection("Authentication:Google");


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

//
//  AI SERVICE
//
builder.Services.AddHttpClient<IAiRecommendationService, AiRecommendationService>(client =>
{
    client.BaseAddress = new Uri("https://api.groq.com/");
    client.Timeout = TimeSpan.FromSeconds(90);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 1;
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(85);
    options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(40);
    options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(90); // має бути > AttemptTimeout * 2
});

builder.Services.AddHttpClient<IWeatherApiService, WeatherApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/");
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
});

builder.Services.AddHttpClient<ICountryInfoService, CountryInfoService>(client =>
{
    client.BaseAddress = new Uri("https://restcountries.com/");
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 2;
});

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>(client =>
{
    client.BaseAddress = new Uri("https://bank.gov.ua/");
    client.Timeout = TimeSpan.FromSeconds(20);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 2;
});

builder.Services.AddHttpClient<INominatimService, NominatimService>(client =>
{
    client.BaseAddress = new Uri("https://nominatim.openstreetmap.org/");
    client.Timeout = TimeSpan.FromSeconds(25);
    client.DefaultRequestHeaders.Add("User-Agent", "TravelManager/1.0 (university project)");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(40);
});

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
    c.RoutePrefix = "swagger";
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
await app.SeedDemoDataAsync("andrii.minich@oa.edu.ua"); // email свого акаунту


app.Run();
