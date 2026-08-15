using System.Text.Json.Serialization;

namespace HaYTooLWeather.Models;

/// <summary>
/// Open-Meteo Geocoding API'den dönen konum modeli.
/// </summary>
public class GeoLocation
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("latitude")]
    public double Latitude { get; set; }

    [JsonPropertyName("longitude")]
    public double Longitude { get; set; }

    [JsonPropertyName("country")]
    public string Country { get; set; } = string.Empty;

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; } = string.Empty;

    [JsonPropertyName("admin1")]
    public string? Admin1 { get; set; } // İl / Bölge

    [JsonPropertyName("admin2")]
    public string? Admin2 { get; set; } // İlçe

    public string DisplayName
    {
        get
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(Name)) parts.Add(Name);
            if (!string.IsNullOrWhiteSpace(Admin2) && Admin2 != Name) parts.Add(Admin2);
            if (!string.IsNullOrWhiteSpace(Admin1) && Admin1 != Name) parts.Add(Admin1);
            if (!string.IsNullOrWhiteSpace(Country)) parts.Add(Country);
            return string.Join(", ", parts);
        }
    }
}

public class GeocodingResponse
{
    [JsonPropertyName("results")]
    public List<GeoLocation>? Results { get; set; }
}
