using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(201903310217)]
    public class Mig20190331RegularTask : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("Money").Table("RegularTask").Exists())
            {
                return;
            }
            Execute.Sql(@"
ALTER TABLE [Money].[Payment]
ADD [TaskId] int NULL
GO

CREATE TABLE [Money].[RegularTask] (
  [Id] int IDENTITY(1, 1) NOT NULL,
  [UserId] int NOT NULL,
  [TaskId] int NOT NULL,
  [Name] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
  [TypeId] int NOT NULL,
  [TimeId] int NOT NULL,
  [TimeValue] int NULL,
  [DateFrom] date NOT NULL,
  [DateTo] date NULL,
  CONSTRAINT [RegularTask_pk] PRIMARY KEY CLUSTERED ([Id])
)
ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [RegularTask_uq] ON [Money].[RegularTask]
  ([UserId], [TaskId])
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

ALTER TABLE [Money].[RegularTask]
ADD CONSTRAINT [RegularTask_UserId_fk] FOREIGN KEY ([UserId]) 
  REFERENCES [System].[User] ([Id]) 
  ON UPDATE NO ACTION
  ON DELETE NO ACTION
GO

ALTER TABLE [Money].[RegularTask]
ADD [RunTime] datetime NULL
GO

ALTER TABLE [Money].[RegularTask]
ALTER COLUMN [RunTime] datetime NOT NULL
GO

ALTER TABLE [Money].[Payment]
ADD [CreatedTaskId] int NULL
GO

ALTER TABLE [Money].[RegularTask]
ALTER COLUMN [RunTime] datetime
GO
");
        }

        public override void Down()
        {

        }
    }
}
