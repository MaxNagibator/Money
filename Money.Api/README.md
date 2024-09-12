### Обновить/создать БД

dotnet ef database update -c Money.Api.Data.ApplicationDbContext --project Money.Api

### Добавить миграцию

dotnet ef migrations add CreateCategoryTable -c Money.Api.Data.ApplicationDbContext --project Money.Api
