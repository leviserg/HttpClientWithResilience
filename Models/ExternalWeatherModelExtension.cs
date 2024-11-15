namespace HttpClientWithResilience.Models
{
    public static class ExternalWeatherModelExtension
    {
        public static CustomWeatherModel ToCustomWeatherModel(this ExternalWeatherModel model) {
            return new CustomWeatherModel
            {
                Name = model.Name,
                Temp = (model?.Main?.Temp is null) ? null : Math.Round(model.Main.Temp.GetValueOrDefault() - 273.15,2),
                WindSpeed = model?.Wind?.Speed,
                WindDeg = model?.Wind?.Deg,
                WindDirection = GetDirection(model?.Wind?.Deg),
                Pressure = model?.Main?.Pressure,
                Humidity = model?.Main?.Humidity,
                Description = model?.Weather?.FirstOrDefault()?.Description
            };
        }

        private static string GetDirection(int? winDeg) {

            if (!winDeg.HasValue) {
                return "Unknown";
            }

            string[] directions = { "North", "North-East", "East", "South-East", "South", "South-West", "West", "North-West" };
            winDeg = winDeg % 360; // Normalize degree to be within the 0-359 range.

            int index = (int)((winDeg + 22.5) / 45);
            return directions[index % 8];
        }
    }
}
