using System;
using Common.Enums;

namespace Common
{
    public class RegularTask
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public RegularTaskTimeTypes TimeType { get; set; }

        public RegularTaskTypes Type { get; set; }

        public int? TimeValue { get; set; }
        
        public DateTime DateFrom { get; set; }

        public DateTime? DateTo { get; set; }

        public Payment Payment { get; set; }

        public DateTime? RunTime { get; set; }
            }
}
