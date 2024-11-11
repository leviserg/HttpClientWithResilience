using HttpClientWithResilience.Services;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Fallback;
using Polly.Timeout;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddScoped<ExternalWeatherService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
