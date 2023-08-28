namespace MBATestTask.Models;

public class City
{
    public long Id { get; set; }

    public string? Name { get; set; }

    public string? Country { get; set; }

    public virtual ICollection<Forecast> Forecasts { get; set; } = new List<Forecast>();
}
