using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class CreateCarEventRequest : TokenRequest
    {
        [DataMember]
        public CarEventValue CarEvent { get; set; }

        [DataMember]
        public int CarId { get; set; }

        [DataContract]
        public class CarEventValue
        {
            [DataMember]
            public string Title { get; set; }

            [DataMember]
            public double Date { get; set; }

            [DataMember]
            public CarEventTypes Type { get; set; }

            [DataMember]
            public string Comment { get; set; }

            [DataMember]
            public decimal? Mileage { get; set; }
        }
    }
}
