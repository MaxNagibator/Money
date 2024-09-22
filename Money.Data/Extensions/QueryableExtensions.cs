﻿using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Money.Data.Entities;

namespace Money.Data.Extensions;

public static class QueryableExtensions
{
    public static T? SingleOrDefault<T>(this IQueryable<T> queryable, int? userId, int entityId)
        where T : class, IUserEntity
    {
        return queryable.SingleOrDefault(GetUserEntity<T>(userId, entityId));
    }

    public static Task<T?> SingleOrDefaultAsync<T>(this IQueryable<T> queryable, int? userId, int entityId, CancellationToken cancellationToken = default)
        where T : class, IUserEntity
    {
        return queryable.SingleOrDefaultAsync(GetUserEntity<T>(userId, entityId), cancellationToken);
    }

    public static IQueryable<T> IsUserEntity<T>(this IQueryable<T> queryable, int? userId)
        where T : class, IUserEntity
    {
        return queryable.Where(entity => entity.UserId == userId);
    }

    public static IQueryable<T> IsUserEntity<T>(this IQueryable<T> queryable, int? userId, int? entityId)
        where T : class, IUserEntity
    {
        return queryable.Where(GetUserEntity<T>(userId, entityId));
    }

    public static Expression<Func<T, bool>> GetUserEntity<T>(int? userId, int? id)
        where T : class, IUserEntity
    {
        return entity => entity.Id == id && entity.UserId == userId;
    }
}
