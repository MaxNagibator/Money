```genericsql
ALTER TABLE [System].[User]
ADD Guid uniqueidentifier NULL;
UPDATE [System].[User]
SET Guid = NEWID(); 
```

```genericsql
BEGIN;
TRUNCATE "AspNetUsers" CASCADE;
TRUNCATE "domain_users" CASCADE;
TRUNCATE "debt_owners" CASCADE;
TRUNCATE "debts" CASCADE;
TRUNCATE "categories" CASCADE;
TRUNCATE "operations" CASCADE;
TRUNCATE "fast_operations" CASCADE;
TRUNCATE "places" CASCADE;
TRUNCATE "regular_operations" CASCADE;
END;
```

```genericsql
ALTER TABLE Money.RegularTask
    ADD Sum decimal(18, 2), CategoryId int, Comment nvarchar(4000), PlaceId int;

UPDATE Money.RegularTask
SET Sum        = p.Sum,
    CategoryId = p.CategoryId,
    Comment    = p.Comment,
    PlaceId    = p.PlaceId
    FROM Money.RegularTask r
         JOIN Money.Payment p ON r.TaskId = p.TaskId and r.UserId = p.UserId;

ALTER TABLE Money.RegularTask
DROP COLUMN Sum, CategoryId, Comment, PlaceId;

ALTER TABLE [System].[User]
DROP COLUMN Guid;
```
