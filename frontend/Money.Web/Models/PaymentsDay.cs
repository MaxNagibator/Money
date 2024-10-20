namespace Money.Web.Models;

public class PaymentsDay
{
    public DateTime Date { get; set; }

    public List<Payment> Payments { get; set; } = [];

    public decimal CalculateSum(int paymentTypeId)
    {
        return Payments.Where(x => x.IsDeleted == false && x.Category.PaymentTypeId == paymentTypeId)
            .Sum(x => x.Sum);
    }
}
