namespace Money.Web.Models;

public class Car
{
    public int? Id { get; set; }

    public required string Name { get; set; }

    public List<CarEvent>? Events { get; set; }
}
