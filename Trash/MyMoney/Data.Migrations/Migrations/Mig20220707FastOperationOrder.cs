using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(2022007070217)]
    public class Mig20220707FastOperationOrder : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("Money").Table("FastOperation").Column("Order").Exists())
            {
                return;
            }
            Execute.Sql(@"ALTER TABLE [Money].[FastOperation]
ADD [Order] int NULL
GO
");
        }

        public override void Down()
        {

        }
    }
}
