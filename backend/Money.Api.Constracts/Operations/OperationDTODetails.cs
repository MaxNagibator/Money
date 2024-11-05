namespace Money.Api.Constracts.Operations
{
    public class OperationDTODetails
    {
        public required int CategoryId { get; set; }
        public decimal Sum { get; set; }
        public string? Comment { get; set; }
        public string? Place { get; set; }
        public DateTime Date { get; set; }
    }
}