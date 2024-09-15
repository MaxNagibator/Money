using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(202204030217)]
    public class Mig20220403FastOperation : Migration
    {
        public override void Up()
        {
            if (Schema.Schema("Money").Table("FastOperation").Exists())
            {
                return;
            }
            Execute.Sql(@"
CREATE TABLE [Money].[FastOperation] (
  [Id] int IDENTITY(1, 1) NOT NULL,
  [UserId] int NOT NULL,
  [FastOperationId] int NOT NULL,
  [Name] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
    [Sum] [decimal](18, 2) NOT NULL,
    [CategoryId] [int] NULL,
    [TypeId] [int] NOT NULL,
    [Comment] [nvarchar](4000) NULL,
    [PlaceId] [int] NULL,
  CONSTRAINT [FastOperation_pk] PRIMARY KEY CLUSTERED ([Id])
)
ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [FastOperation_uq] ON [Money].[FastOperation]
  ([UserId], [FastOperationId])
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

ALTER TABLE [Money].[FastOperation]
ADD CONSTRAINT [FastOperation_UserId_fk] FOREIGN KEY ([UserId]) 
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
