```genericsql
ALTER TABLE [System].[User]
ADD Guid uniqueidentifier NULL;
UPDATE [System].[User]
SET Guid = NEWID(); 
```
