namespace HttpClientWithResilience.Models
{
    public class CustomWeatherModel
    {
        public double? Temp { get; set; }
        public double? WindSpeed { get; set; }
        public int? WindDeg { get; set; }
        public string? WindDirection { get; set; }
        public int? Pressure { get; set; }
        public int? Humidity { get; set; }
        public  string? Name { get; set; }
        public string? Description { get; set; }
    }
}
