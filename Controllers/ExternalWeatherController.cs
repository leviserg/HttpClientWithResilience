using HttpClientWithResilience.Models;
using HttpClientWithResilience.Services;
using Microsoft.AspNetCore.Mvc;

namespace HttpClientWithResilience.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExternalWeatherController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly ExternalWeatherService _service;

        public ExternalWeatherController(
            ILogger<WeatherForecastController> logger,
            ExternalWeatherService service
            )
        {
            _logger = logger;
            _service = service;
        }

        [HttpGet(Name = "GetWeather/{city}")]
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
    }
}
