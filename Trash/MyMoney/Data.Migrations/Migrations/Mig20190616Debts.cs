using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(201806160217)]
    public class Mig20190616Debts : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("Money").Table("DebtUser").Exists())
            {
                return;
            }
            Execute.Sql(@"CREATE TABLE [Money].[DebtUser] (
  [Id] int IDENTITY(1, 1) NOT NULL,
  [UserId] int NOT NULL,
  [DebtUserId] int NOT NULL,
  [Name] nvarchar(2000) COLLATE Cyrillic_General_CI_AS NOT NULL,
  CONSTRAINT [DebtUser_pk] PRIMARY KEY CLUSTERED ([Id])
)
ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [DebtUser_uq] ON [Money].[DebtUser]
  ([UserId], [DebtUserId])
WITH (
  PAD_INDEX = OFF,
  IGNORE_DUP_KEY = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [PRIMARY]
GO

INSERT INTO Money.DebtUser (UserId, Name, DebtUserId)
SELECT D.UserId
	,D.[Name]
	,ROW_NUMBER() OVER(PARTITION BY UserId ORDER BY UserId DESC) AS Row  
FROM Money.Debt AS D
GROUP BY D.UserId, D.[Name]
ORDER BY D.UserId
GO

ALTER TABLE [Money].[Debt]
ADD [DebtUserId] int NULL
GO

UPDATE Money.Debt
SET DebtUserId = U.DebtUserId
FROM Money.Debt AS D
	INNER JOIN Money.DebtUser AS U ON U.UserId = D.UserId
    	AND U.[Name] = D.[Name]

GO

ALTER TABLE [Money].[Debt]
ALTER COLUMN [DebtUserId] int NOT NULL
GO

ALTER TABLE [Money].[Debt]
DROP COLUMN [Name]
GO

ALTER TABLE [Money].[DebtUser]
ADD CONSTRAINT [DebtUser_UserId_fk] FOREIGN KEY ([UserId]) 
  REFERENCES [System].[User] ([Id]) 
  ON UPDATE NO ACTION
  ON DELETE NO ACTION
GO
");
        }

        public override void Down()
        {

        }
    }
}
