using System;
using System.Configuration;
using System.Reflection;
using FluentMigrator;
using FluentMigrator.Runner;
using FluentMigrator.Runner.Announcers;
using FluentMigrator.Runner.Initialization;

namespace Data.Migrations
{
    public class Runner
    {
        private static MigrationRunner _migrationRunner;
        public static string ConnectionString;
        public class MigrationOptions : IMigrationProcessorOptions
        {
            public bool PreviewOnly { get; set; }

            public int Timeout { get; set; }
        }

        public static void Initialize(string connectionString)
        {
            ConnectionString = connectionString;
            var announcer = new TextWriterAnnouncer(Console.Out);
            var assembly = Assembly.GetExecutingAssembly();

            var migrationContext = new RunnerContext(announcer)
            {
                Namespace = "Data.Migrations",
                NestedNamespaces = true
            };

            var timeoutString = ConfigurationManager.AppSettings["MigrationOptionTimeout"];
            int timeout;
            if (String.IsNullOrEmpty(timeoutString))
            {
                timeout = 600;
            }
            else
            {
                int timeout2;
                timeout = !Int32.TryParse(timeoutString, out timeout2) ? 600 : timeout2;
            }

            var options = new MigrationOptions { PreviewOnly = false, Timeout = timeout };
            var factory = new FluentMigrator.Runner.Processors.SqlServer.SqlServer2012ProcessorFactory();
            var processor = factory.Create(connectionString, announcer, options);
            _migrationRunner = new MigrationRunner(assembly, migrationContext, processor);
        }

        public static void MigrateToLatest()
        {
            _migrationRunner.MigrateUp(true);
        }

        public static void MigrateTo(long version)
        {
            _migrationRunner.MigrateUp(version, true);
        }

        public static void ListMigrations()
        {
            _migrationRunner.ValidateVersionOrder();
        }

        public static void MigrateDownToVersion(long migrationVesrion)
        {
            _migrationRunner.MigrateDown(migrationVesrion, true);
        }
    }
}