using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRequest.Cars
{
    [DataContract]
    public class UpdateCarEventRequest : TokenRequest
    {
        [DataMember]
        public CarEventValue CarEvent { get; set; }

        [DataContract]
        public class CarEventValue
        {
            [DataMember]
            public int Id { get; set; }

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
