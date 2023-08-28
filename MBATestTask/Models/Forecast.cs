namespace MBATestTask.Models;

public class Forecast
{
    public long Id { get; set; }

    public long? CityId { get; set; }

    public string? DateFrom { get; set; }

    public string? DateTo { get; set; }

    public double? Temperature { get; set; }

    public double? Pressure { get; set; }

    public virtual City? City { get; set; }
}
