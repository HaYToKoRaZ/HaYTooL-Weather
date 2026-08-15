using System.Text.Json.Serialization;

namespace HaYTooLWeather.Models;

/// <summary>
/// Open-Meteo hava durumu API yanıtı ve işlenmiş hava durumu verileri.
/// </summary>
public class WeatherApiResponse
{
    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("timezone")]
    public string? Timezone { get; set; }

    [JsonPropertyName("current")]
    public CurrentWeatherDto? Current { get; set; }

    [JsonPropertyName("hourly")]
    public HourlyWeatherDto? Hourly { get; set; }

    [JsonPropertyName("daily")]
    public DailyWeatherDto? Daily { get; set; }
}

public class CurrentWeatherDto
{
    [JsonPropertyName("time")]
    public string Time { get; set; } = string.Empty;

    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    [JsonPropertyName("relative_humidity_2m")]
    public int RelativeHumidity { get; set; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperature { get; set; }

    [JsonPropertyName("is_day")]
    public int IsDay { get; set; }

    [JsonPropertyName("precipitation")]
    public double Precipitation { get; set; }

    [JsonPropertyName("weather_code")]
    public int WeatherCode { get; set; }

    [JsonPropertyName("wind_speed_10m")]
    public double WindSpeed { get; set; }
}

public class HourlyWeatherDto
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("temperature_2m")]
    public List<double>? Temperature { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }
}

public class DailyWeatherDto
{
    [JsonPropertyName("time")]
    public List<string>? Time { get; set; }

    [JsonPropertyName("weather_code")]
    public List<int>? WeatherCode { get; set; }

    [JsonPropertyName("temperature_2m_max")]
    public List<double>? TemperatureMax { get; set; }

    [JsonPropertyName("temperature_2m_min")]
    public List<double>? TemperatureMin { get; set; }

    [JsonPropertyName("precipitation_probability_max")]
    public List<int>? PrecipitationProbabilityMax { get; set; }
}

/// <summary>
/// Arayüzde ve tepsisinde gösterilmeye hazır işlenmiş hava durumu nesnesi.
/// </summary>
public class ProcessedWeatherData
{
    public string LocationName { get; set; } = string.Empty;
    public double Temperature { get; set; }
    public double ApparentTemperature { get; set; }
    public int Humidity { get; set; }
    public double WindSpeed { get; set; }
    public double Precipitation { get; set; }
    public int WeatherCode { get; set; }
    public bool IsDay { get; set; } = true;
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    public List<ProcessedDailyForecast> DailyForecasts { get; set; } = new();
    public List<ProcessedHourlyForecast> HourlyForecasts { get; set; } = new();
}

public class ProcessedDailyForecast
{
    public DateTime Date { get; set; }
    public int WeatherCode { get; set; }
    public double MinTemp { get; set; }
    public double MaxTemp { get; set; }
    public int RainChance { get; set; }
}

public class ProcessedHourlyForecast
{
    public DateTime Time { get; set; }
    public double Temp { get; set; }
    public int WeatherCode { get; set; }
}
