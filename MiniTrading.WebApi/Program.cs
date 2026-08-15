using Microsoft.EntityFrameworkCore;
using MiniTrading.Application.Interfaces;
using MiniTrading.Application.Services;
using MiniTrading.Domain.Constants;
using MiniTrading.Infrastructure.Persistence;
using MiniTrading.Infrastructure.Persistence.Repositories;
using MiniTrading.WebApi.Hubs;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration.GetConnectionString(AppConstant.ConfigurationKeys.DefaultConnection);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient<IAuthService, AuthService>();
builder.Services.AddSingleton<IPriceCache, PriceCache>();
builder.Services.AddScoped<ITradeRepository, TradeRepository>();
builder.Services.AddScoped<ITradingService, TradingService>();
builder.Services.AddSingleton<IPricePublisher, SignalRPricePublisher>();

builder.Services.AddHostedService<WebSocketPriceStreamService>();

builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy(AppConstant.Cors.AllowFrontendPolicy, policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>
    {
        options.RouteTemplate = AppConstant.SwaggerDoc.RouteTemplate;
    });
    app.MapScalarApiReference(options =>
    {
        options.WithTitle(AppConstant.SwaggerDoc.ApiTitle)
               .WithTheme(ScalarTheme.Purple);
    });
}

app.UseCors(AppConstant.Cors.AllowFrontendPolicy);
app.UseAuthorization();

app.MapControllers();
app.MapHub<TradingHub>(AppConstant.SignalR.HubEndpoint);

app.Run();