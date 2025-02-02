```genericsql
ALTER TABLE [System].[User]
ADD Guid uniqueidentifier NULL;
UPDATE [System].[User]
SET Guid = NEWID(); 
```

```genericsql
truncate "AspNetUsers" CASCADE;
truncate "domain_users" CASCADE;
truncate "debt_owners" CASCADE;
truncate "debts" CASCADE;
truncate "categories" CASCADE;
truncate "operations" CASCADE;
truncate "fast_operations" CASCADE;
```
