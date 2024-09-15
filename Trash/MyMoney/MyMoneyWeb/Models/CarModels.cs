using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Common.Enums;

namespace MyMoneyWeb.Models
{
    public class CarsModel
    {
        public List<CarValue> Cars { get; set; }
        public class CarValue
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }

    public class CarDetailsModel
    {
        public CarValue Car { get; set; }

        public class CarValue
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public List<CarEventValue> Events { get; set; }
        }

        public class CarEventValue
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public CarEventTypes Type { get; set; }
            public string Comment { get; set; }
            public decimal? Mileage { get; set; }
            public DateTime Date { get; set; }
        }
    }
}
