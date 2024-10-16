namespace Money.Web.Models;

public class PaymentsDay
{
    public DateTime Date { get; set; }

    public List<Payment> Payments { get; set; } = [];
}
