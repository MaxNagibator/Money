namespace Money.Business.Models;

public class CarEvent
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public CarEventTypes Type { get; set; }

    public string? Comment { get; set; }

    public decimal? Mileage { get; set; }

    public DateTime Date { get; set; }
}
