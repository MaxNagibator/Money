using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentMigrator;

namespace Data.Migrations
{
    [Migration(202212060217)]
    public class Mig20221206DocumentTemplateGroups : Migration
    {
        public override void Up()
        {
            Execute.Sql(@"
CREATE TABLE [Money].[DocumentTemplateGroup] (
  [Id] int IDENTITY(1, 1) NOT NULL,
  [UserId] int NOT NULL,
  [DocumentTemplateGroupId] int NOT NULL,
  [Name] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
  [ReplaceWords] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
  CONSTRAINT [DocumentTemplateGroup_pk] PRIMARY KEY CLUSTERED ([Id])
)
ON [PRIMARY]
GO

CREATE UNIQUE NONCLUSTERED INDEX [DocumentTemplateGroup_uq] ON [Money].[DocumentTemplateGroup]
  ([UserId], [DocumentTemplateGroupId])
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

CREATE TABLE [Money].[DocumentTemplate] (
  [Id] int IDENTITY(1, 1) NOT NULL,
  [DocumentTemplateGroupId] int NOT NULL,
  [Name] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
  [FileName] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
  [DownloadFileName] nvarchar(max) COLLATE Cyrillic_General_CI_AS NOT NULL,
  CONSTRAINT [DocumentTemplate_pk] PRIMARY KEY CLUSTERED ([Id])
)
ON [PRIMARY]
GO
");
        }

        public override void Down()
        {

        }
    }
}
