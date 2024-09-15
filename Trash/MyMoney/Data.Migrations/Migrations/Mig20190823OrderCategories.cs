using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(201908230217)]
    public class Mig20190823OrderCategories : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("Money").Table("Category").Column("Order").Exists())
            {
                return;
            }
            Execute.Sql(@"ALTER TABLE [Money].[Category]
ADD  [Order] int NULL
GO
");
        }

        public override void Down()
        {

        }
    }
}
