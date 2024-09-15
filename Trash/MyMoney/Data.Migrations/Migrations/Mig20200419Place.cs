using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(202004190217)]
    public class Mig20200419Place : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("Money").Table("Place").Exists())
            {
                return;
            }

            Execute.Sql(@"CREATE TABLE [Money].[Place] (
  [Id] int IDENTITY(1, 1) NOT NULL,
  [UserId] int NOT NULL,
  [Name] nvarchar(500) COLLATE Cyrillic_General_CI_AS NOT NULL,
  [Description] nvarchar(4000) COLLATE Cyrillic_General_CI_AS NULL,
  [LastUsedDate] datetime NULL,
  [PlaceId] int NOT NULL,
  CONSTRAINT [Place_pk] PRIMARY KEY CLUSTERED ([Id]),
  CONSTRAINT [Place_UserId_fk] FOREIGN KEY ([UserId]) 
  REFERENCES [System].[User] ([Id]) 
  ON UPDATE CASCADE
  ON DELETE CASCADE
)
ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [Place_UserId_Name_uq] ON [Money].[Place]
  ([UserId], [Name])
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

CREATE UNIQUE NONCLUSTERED INDEX [Place_UserId_PlaceId_uq] ON [Money].[Place]
  ([UserId], [PlaceId])
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

ALTER TABLE [Money].[Payment]
ADD [PlaceId] int NULL
GO
");
        }

        public override void Down()
        {

        }
    }
}
