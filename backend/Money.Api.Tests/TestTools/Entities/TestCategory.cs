using Money.Business.Enums;
using Money.Data.Entities;
using Money.Data.Extensions;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
/// Категория.
/// </summary>
public class TestCategory : TestObject
{
    public TestCategory(TestUser user)
    {
        User = user;

        Name = TestRandom.GetString("Category");
        OperationType = TestRandom.GetEnum<OperationTypes>();
    }

    /// <summary>
    /// Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    /// Наименование.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Родительская категория.
    /// </summary>
    public TestCategory? Parent { get; set; }

    /// <summary>
    /// Тип.
    /// </summary>
    public OperationTypes OperationType { get; private set; }

    /// <summary>
    /// Пользователь.
    /// </summary>
    public TestUser User { get; }

    /// <summary>
    /// Удалена.
    /// </summary>
    public bool IsDeleted { get; set; }

    public TestCategory SetParent(TestCategory category)
    {
        Parent = category;
        return this;
    }

    public TestCategory SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestCategory SetOperationType(OperationTypes value)
    {
        OperationType = value;
        return this;
    }

    public TestOperation WithOperation()
    {
        var obj = new TestOperation(this);
        obj.Attach(Environment);
        return obj;
    }

    public TestFastOperation WithFastOperation()
    {
        var obj = new TestFastOperation(this);
        obj.Attach(Environment);
        return obj;
    }

    public TestRegularOperation WithRegularOperation()
    {
        var obj = new TestRegularOperation(this);
        obj.Attach(Environment);
        return obj;
    }

    private void FillDbProperties(Category obj)
    {
        obj.Name = Name;
        obj.TypeId = (int)OperationType;
        obj.UserId = User.Id;
        obj.ParentId = Parent?.Id;
        obj.IsDeleted = IsDeleted;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            var dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            var categoryId = dbUser.NextCategoryId;
            dbUser.NextCategoryId++; // todo обработать канкаренси

            var obj = new Category
            {
                Id = categoryId,
                Name = "",
            };

            FillDbProperties(obj);
            Environment.Context.Categories.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            var obj = Environment.Context.Categories
                .IsUserEntity(User.Id, Id)
                .First();

            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}
