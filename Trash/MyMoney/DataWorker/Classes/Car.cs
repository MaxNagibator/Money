using System.Collections.Generic;

namespace DataWorker.Classes
{
    public class Car
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public List<CarEvent> Events { get; set; }
    }
}
