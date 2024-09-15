using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(201804100217)]
    public class Mig20180410Init : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"EXEC sp_rename '[Money].[Payment].[IsIncome]', 'TypeId', 'COLUMN'
GO

ALTER TABLE [Money].[Payment]
DROP CONSTRAINT [DF__Payment__IsIncom__0F975522]
GO

ALTER TABLE [Money].[Payment]
ALTER COLUMN [TypeId] int NOT NULL
GO

ALTER TABLE [Money].[Payment]
ADD CONSTRAINT [DF__Payment__IsIncom__0F975522] DEFAULT 0 FOR [TypeId]
GO

ALTER TABLE [Money].[Payment]
DROP CONSTRAINT [DF__Payment__IsIncom__0F975522]
GO

ALTER TABLE [Money].[Category]
ADD [TypeId] int NULL
GO

UPDATE Money.Payment
SET TypeId = 1
WHERE TypeId = 0
GO

UPDATE Money.Category
SET TypeId = 1
GO

ALTER TABLE [Money].[Category]
ALTER COLUMN [TypeId] int NOT NULL
GO

INSERT INTO Money.Category(UserId,CategoryId, Name, TypeId)
SELECT UserId, MAX(CategoryId)+1, 'Базовая', 2
FROM Money.Category
GROUP BY UserId
");
        }

        public override void Down()
        {

        }
    }
}
