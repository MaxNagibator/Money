using System.Linq.Expressions;

namespace Money.Data.Extensions;

public static class QueryableExtensions
{
    public static T First<T>(this IQueryable<T> queryable, int? userId, int? entityId)
        where T : UserEntity
    {
        return queryable.First(GetUserEntity<T>(userId, entityId));
    }

    public static T? FirstOrDefault<T>(this IQueryable<T> queryable, int? userId, int? entityId)
        where T : UserEntity
    {
        return queryable.FirstOrDefault(GetUserEntity<T>(userId, entityId));
    }

    public static Task<T?> FirstOrDefaultAsync<T>(this IQueryable<T> queryable, int? userId, int? entityId, CancellationToken cancellationToken = default)
        where T : UserEntity
    {
        return queryable.FirstOrDefaultAsync(GetUserEntity<T>(userId, entityId), cancellationToken);
    }

    public static IQueryable<T> IsUserEntity<T>(this IQueryable<T> queryable, int? userId)
        where T : UserEntity
    {
        return queryable.Where(entity => entity.UserId == userId);
    }

    public static IQueryable<T> IsUserEntity<T>(this IQueryable<T> queryable, int? userId, int? entityId)
        where T : UserEntity
    {
        return queryable.Where(GetUserEntity<T>(userId, entityId));
    }

    public static Expression<Func<T, bool>> GetUserEntity<T>(int? userId, int? id)
        where T : UserEntity
    {
        return entity => entity.Id == id && entity.UserId == userId;
    }
}
