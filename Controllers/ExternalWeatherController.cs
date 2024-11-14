using HttpClientWithResilience.Models;
using HttpClientWithResilience.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;

namespace HttpClientWithResilience.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExternalWeatherController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ExternalWeatherService _service;
        private readonly IOutputCacheStore _cache;
        private const string cityWeatheCacheTag = "cityweather";

        public ExternalWeatherController(
            ILogger<WeatherForecastController> logger,
            ExternalWeatherService service,
            IOutputCacheStore cache
            )
        {
            _logger = logger;
            _service = service;
            _cache = cache;
        }

        [HttpGet(Name = "GetWeather/{city}")]
        //[OutputCache(PolicyName = "customcachepolicy", Duration = 20, NoStore = false)]
        [OutputCache(PolicyName = "externalapi", Tags = [cityWeatheCacheTag])]
        public async Task<ActionResult<ExternalWeatherModel?>> GetCityWeather(string city)
        {
            try
            {
                var weather = await _service.GetCurrentWetherAsync(city);
                return Ok(weather);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpPost(Name = "ResetCache")]
        public async Task<ActionResult> ResetCache(CancellationToken ct)
        {
            try
            {
                await _cache.EvictByTagAsync(cityWeatheCacheTag, ct);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
