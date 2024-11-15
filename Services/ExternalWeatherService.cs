using HttpClientWithResilience.Models;
using System.Net;

namespace HttpClientWithResilience.Services
{
    public class ExternalWeatherService
    {
        private readonly HttpClient _httpClient;
        private string openWeatherApiKey => Environment.GetEnvironmentVariable("OpenWeatherApiKey") ?? string.Empty;
        // private readonly ResiliencePipelineProvider<string> _pipelineProvider; // * w/o .AddStandardResilienceHndler() in Program.cs

        public ExternalWeatherService(
            // ResiliencePipelineProvider<string> pipelineProvider, // * w/o .AddStandardResilienceHndler() in Program.cs
            HttpClient httpClient
        )
        {
            _httpClient = httpClient;
            // _pipelineProvider = pipelineProvider; // * w/o .AddStandardResilienceHndler() in Program.cs
        }

        public async Task<CustomWeatherModel> GetCurrentWetherAsync(string city)
        {

            var url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={openWeatherApiKey}";

            // var pipeline = _pipelineProvider.GetPipeline("default") // * w/o .AddStandardResilienceHndler() in Program.cs

            /* * w/o .AddStandardResilienceHndler() in Program.cs
            var weatherResponse = await pipeline.ExecuteAsync(
                async cancellationToken => await _httpClient.GetAsync(url, cancellationToken)
            );
            */

            var weatherResponse = await _httpClient.GetAsync(url); // * w .AddStandardResilienceHndler() in Program.cs

            if (weatherResponse.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Requested city not found");
            }

            var content = await weatherResponse.Content.ReadFromJsonAsync<ExternalWeatherModel?>();

            return (content == null) ? new CustomWeatherModel() : content.ToCustomWeatherModel();
        }
    }
}
