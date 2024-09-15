using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(202105180217)]
    public class Mig20210518Index : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
	CREATE NONCLUSTERED INDEX [Payment_idx] ON Money.Payment (UserId, TaskId, Date, PlaceId)
");
        }

        public override void Down()
        {

        }
    }
}
