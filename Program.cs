using HttpClientWithResilience.Cache;
using HttpClientWithResilience.Services;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Fallback;
using Polly.Timeout;
using StackExchange.Redis;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region Resilience

// ###### customize some resilience policy ######

builder.Services.AddHttpClient("CustomResilientClient")
    .AddResilienceHandler("normal", b =>
    {
    b.AddFallback(new FallbackStrategyOptions<HttpResponseMessage>()
    {
        FallbackAction = _ => Outcome.FromResultAsValueTask(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
    })
    .AddConcurrencyLimiter(100)
    .AddRetry(new HttpRetryStrategyOptions    
    {
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<TimeoutRejectedException>()
            .Handle<HttpRequestException>()
            .HandleResult(response => response.StatusCode == HttpStatusCode.InternalServerError),
        Delay = TimeSpan.FromSeconds(2),
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential, // Constant, Linear & Exponential options available
        UseJitter = true // randomizing front and back time edges of request
    })
    .AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions())
    .AddTimeout(TimeSpan.FromSeconds(20));
});

// ###### customize some resilience policy assigned to specific class by overriding standard policy ######

builder.Services.AddHttpClient<ExternalWeatherService>("ResilientClientWithOptions") // pass specific handler to assigned class
    .AddStandardResilienceHandler(options =>
    {
        options.Retry.Delay = TimeSpan.FromSeconds(5);
        options.Retry.MaxRetryAttempts = 3;
        options.Retry.BackoffType = DelayBackoffType.Exponential;
        options.Retry.UseJitter = true;
        options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(11);
    }
    );

#endregion

builder.Services.AddScoped<ExternalWeatherService>();

// add cache for response holding

builder.Services.AddOutputCache(options =>
    {
        options.AddPolicy("nocache", x => x.NoCache());
        options.AddPolicy("externalapi", x =>
        {
            x.Expire(TimeSpan.FromSeconds(600));
        });
        options.AddPolicy("customcachepolicy", OutputCacheWithAuthPolicy.Instance);
    })
    .AddStackExchangeRedisOutputCache(options =>
    {
        //x.ConnectionMultiplexerFactory = async () => await ConnectionMultiplexer.ConnectAsync("localhost:6379");
        options.InstanceName = "externalapi"; // key prefix in redis cache db
        options.Configuration = "localhost:6379";
    });

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRouting();

app.UseOutputCache();

app.MapControllers();

app.Run();
