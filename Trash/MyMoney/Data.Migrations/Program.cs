using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace Data.Migrations
{
    class Program
    {
        public static void Main(string[] args)
        {
            bool isSuccess = true;
            bool needSaveInFile = false;
            DateTime? date = null;
            try
            {
                string server;
                string catalog;
                if (!args.Any())
                {
                    server = "localhost";
                    catalog = "openums";
                }
                else
                {
                    server = args[0];
                    catalog = args[1];
                }
                string login = args.Length > 2 ? args[2] : "ums";
                string password = args.Length > 3 ? args[3] : "umsforadmin";
                Runner.Initialize("Data Source=" + server + ";Initial Catalog=" + catalog + ";Persist Security Info=True;User ID=" + login + ";Password=" + password + ";");

                var migrationNumberStr = args.Length > 4 ? args[4] : "-1";
                long migrationNumber = -1;
                if (migrationNumberStr == "list")
                {
                    Runner.ListMigrations();
                    return;
                }
                else
                {
                    migrationNumber = Convert.ToInt64(migrationNumberStr);
                }
                var date2 = args.Length > 5 ? args[5] : "0";
                if(date2 != "0")
                {
                    date = DateTime.Parse(args[5]);
                    needSaveInFile = true;
                }
                if(migrationNumber == -1)
                {
                    Runner.MigrateToLatest();
                }
                else
                {
                    Runner.MigrateTo(migrationNumber);
                }
            }
            catch (Exception ex)
            {
                isSuccess = false;
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
            if (needSaveInFile)
            {
                var results = new MigrateResults()
                {
                    MigrationDate = date.Value,
                    Result = isSuccess
                };
                SaveInXml(results);
            }
        }

        private static void SaveInXml(MigrateResults results)
        {
            var serializer = new XmlSerializer(typeof(MigrateResults));
            using (FileStream fs = new FileStream(MigrateResults.FileName, FileMode.Create))
            {
                serializer.Serialize(fs, results);
            } 
        }
    }
}
