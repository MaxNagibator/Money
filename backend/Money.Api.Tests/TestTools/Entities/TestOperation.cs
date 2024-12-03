﻿using Money.Data.Entities;

namespace Money.Api.Tests.TestTools.Entities;

/// <summary>
///     Операция.
/// </summary>
public class TestOperation : TestObject
{
    public TestOperation(TestCategory category)
    {
        IsNew = true;
        Sum = 100;
        Comment = $"P{Guid.NewGuid()}";
        Category = category;
        Date = DateTime.Now.Date;
    }

    /// <summary>
    ///     Идентификатор.
    /// </summary>
    public int Id { get; private set; }

    /// <summary>
    ///     Сумма.
    /// </summary>
    public decimal Sum { get; private set; }

    /// <summary>
    ///     Комментарий.
    /// </summary>
    public string Comment { get; private set; }

    /// <summary>
    ///     Дата.
    /// </summary>
    public DateTime Date { get; private set; }

    /// <summary>
    ///     Категория.
    /// </summary>
    public TestCategory Category { get; }

    /// <summary>
    ///     Пользователь.
    /// </summary>
    public TestUser User => Category.User;

    /// <summary>
    ///     Место.
    /// </summary>
    public TestPlace Place { get; private set; } = null!;

    /// <summary>
    ///     Удалена.
    /// </summary>
    public bool IsDeleted { get; private set; }

    public TestOperation SetIsDeleted()
    {
        IsDeleted = true;
        return this;
    }

    public TestOperation SetDate(DateTime value)
    {
        Date = value;
        return this;
    }

    public TestOperation SetPlace(TestPlace value)
    {
        Place = value;
        return this;
    }

    public TestOperation SetComment(string value)
    {
        Comment = value;
        return this;
    }

    public TestOperation SetSum(decimal value)
    {
        Sum = value;
        return this;
    }

    private void FillDbProperties(Operation obj)
    {
        obj.Comment = Comment;
        obj.Sum = Sum;
        obj.UserId = User.Id;
        obj.CategoryId = Category.Id;
        obj.IsDeleted = IsDeleted;
        obj.Date = Date;
        obj.PlaceId = Place?.Id;
    }

    public override void LocalSave()
    {
        if (IsNew)
        {
            DomainUser dbUser = Environment.Context.DomainUsers.Single(x => x.Id == User.Id);
            int operationId = dbUser.NextOperationId;
            dbUser.NextOperationId++; // todo обработать канкаренси

            Operation obj = new()
            {
                Id = operationId,
            };

            FillDbProperties(obj);
            Environment.Context.Operations.Add(obj);
            IsNew = false;
            Environment.Context.SaveChanges();
            Id = obj.Id;
        }
        else
        {
            Operation obj = Environment.Context.Operations.First(x => x.UserId == User.Id && x.Id == Id);
            FillDbProperties(obj);
            Environment.Context.SaveChanges();
        }
    }
}