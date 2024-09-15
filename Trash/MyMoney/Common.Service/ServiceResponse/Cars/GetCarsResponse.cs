using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Common.Enums;

namespace ServiceRespone.Cars
{
    [DataContract]
    public class GetCarsResponse
    {
        [DataMember]
        public List<CarValue> Cars { get; set; }

        [DataContract]
        public class CarValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Name { get; set; }

            [DataMember]
            public List<CarEventValue> Events { get; set; }
        }

        [DataContract]
        public class CarEventValue
        {
            [DataMember]
            public int Id { get; set; }

            [DataMember]
            public string Title { get; set; }

            [DataMember]
            public CarEventTypes Type { get; set; }

            [DataMember]
            public string Comment { get; set; }

            [DataMember]
            public decimal? Mileage { get; set; }

            [DataMember]
            public double Date { get; set; }
        }
    }
}
