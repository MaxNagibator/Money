namespace Money.Web.Models;

public class PaymentsDay
{
    public DateTime Date { get; set; }

    public List<Payment> Payments { get; set; } = [];

    public decimal CalculateSum(PaymentTypes.Value paymentType)
    {
        return Payments.Where(x => x.IsDeleted == false && x.Category.PaymentType == paymentType)
            .Sum(x => x.Sum);
    }

    public void AddPayment(Payment payment)
    {
        Payments.Add(payment);
    }
}
