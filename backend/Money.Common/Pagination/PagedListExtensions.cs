using Microsoft.EntityFrameworkCore;

namespace Money.Common.Pagination;

/// <summary>
/// Предоставляет вспомогательные методы для интерфейса <see cref="IPagedList{T}" />.
/// </summary>
public static class PagedListExtensions
{
    /// <summary>
    /// Создает пустой экземпляр <see cref="IPagedList{T}" />.
    /// </summary>
    /// <typeparam name="T">Тип данных для постраничного отображения.</typeparam>
    /// <returns>Пустой экземпляр <see cref="IPagedList{T}" />.</returns>
    public static IPagedList<T> Empty<T>()
    {
        return new PagedList<T>();
    }

    /// <summary>
    /// Создает новый экземпляр <see cref="IPagedList{T}" /> с заданными параметрами.
    /// </summary>
    /// <typeparam name="T">Тип данных для постраничного отображения.</typeparam>
    /// <param name="items">Коллекция элементов.</param>
    /// <param name="pageIndex">Индекс страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="indexFrom">Начальный индекс.</param>
    /// <returns>Экземпляр <see cref="IPagedList{T}" /> с указанными параметрами.</returns>
    public static IPagedList<T> Create<T>(
        IEnumerable<T> items,
        int pageIndex,
        int pageSize,
        int indexFrom)
    {
        return new PagedList<T>(items, pageIndex, pageSize, indexFrom);
    }

    /// <summary>
    /// Преобразует указанную коллекцию в <see cref="IPagedList{T}" /> с указанными параметрами.
    /// </summary>
    /// <typeparam name="T">Тип данных коллекции.</typeparam>
    /// <param name="source">Исходная коллекция для постраничного отображения.</param>
    /// <param name="pageIndex">Индекс страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="indexFrom">Начальный индекс (по умолчанию 0).</param>
    /// <returns>Экземпляр <see cref="IPagedList{T}" />.</returns>
    public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom = 0)
    {
        return Create(source, pageIndex, pageSize, indexFrom);
    }

    /// <summary>
    /// Асинхронно преобразует указанную коллекцию в <see cref="IPagedList{T}" /> с указанными параметрами.
    /// </summary>
    /// <typeparam name="T">Тип данных коллекции.</typeparam>
    /// <param name="source">Исходная коллекция для постраничного отображения.</param>
    /// <param name="pageIndex">Индекс страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="indexFrom">Начальный индекс (по умолчанию 0).</param>
    /// <param name="cancellationToken">Токен отмены для прерывания операции.</param>
    /// <returns>Асинхронный результат, содержащий экземпляр <see cref="IPagedList{T}" />.</returns>
    public static async Task<IPagedList<T>> ToPagedListAsync<T>(
        this IQueryable<T> source,
        int pageIndex,
        int pageSize,
        int indexFrom = 0,
        CancellationToken cancellationToken = default)
    {
        if (indexFrom > pageIndex)
        {
            throw new ArgumentException($"indexFrom: {indexFrom} > pageIndex: {pageIndex}, indexFrom должно быть меньше или равно pageIndex.");
        }

        var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await source.Skip((pageIndex - indexFrom) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return new PagedList<T>(items, pageIndex, pageSize, indexFrom, count);
    }
}
