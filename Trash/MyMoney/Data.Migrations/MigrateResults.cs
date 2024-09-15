using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Migrations
{
    public class MigrateResults
    {
        public DateTime MigrationDate { get; set; }
        public bool Result { get; set; }

        public static string FileName
        {   
            get { return "MigrateResults.xml"; }
        }
    }
}
