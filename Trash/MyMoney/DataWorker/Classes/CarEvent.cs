using Common.Enums;

namespace DataWorker.Classes
{
    public class CarEvent
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public CarEventTypes Type { get; set; }
        public string Comment { get; set; }
        public decimal? Mileage { get; set; }
        public System.DateTime Date { get; set; }
    }
}
