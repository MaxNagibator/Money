namespace Money.Common.Pagination;

/// <summary>
///     Представляет стандартную реализацию интерфейса <see cref="IPagedList{T}" />.
/// </summary>
/// <typeparam name="T">Тип данных для постраничного отображения.</typeparam>
public class PagedList<T> : IPagedList<T>
{
    /// <summary>
    ///     Создает экземпляр с предопределенными параметрами.
    /// </summary>
    /// <param name="source">Источник данных.</param>
    /// <param name="pageIndex">Индекс текущей страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="indexFrom">Начальный индекс.</param>
    /// <param name="count">Общее количество элементов.</param>
    public PagedList(
        IEnumerable<T> source,
        int pageIndex,
        int pageSize,
        int indexFrom,
        int count)
    {
        PageIndex = pageIndex;
        PageSize = pageSize;
        IndexFrom = indexFrom;
        TotalCount = count;
        Items = source.ToList();
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
    }

    /// <summary>
    ///     Инициализирует новый экземпляр класса <see cref="PagedList{T}" />.
    /// </summary>
    /// <param name="source">Источник данных.</param>
    /// <param name="pageIndex">Индекс текущей страницы.</param>
    /// <param name="pageSize">Размер страницы.</param>
    /// <param name="indexFrom">Начальный индекс.</param>
    /// <param name="totalCount">
    ///     Общее количество элементов в коллекции. По умолчанию null, в этом случае количество элементов
    ///     вычисляется по источнику.
    /// </param>
    internal PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom, int? totalCount = null)
    {
        if (indexFrom > pageIndex)
        {
            throw new ArgumentException($"indexFrom: {indexFrom} > pageIndex: {pageIndex}, indexFrom должен быть меньше или равен pageIndex");
        }

        if (source is IQueryable<T> queryable)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;
            TotalCount = totalCount ?? queryable.Count();
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);
            Items = queryable.Skip((PageIndex - IndexFrom) * PageSize).Take(PageSize).ToList();
        }
        else
        {
            List<T> enumerable = source.ToList();
            PageIndex = pageIndex;
            PageSize = pageSize;
            IndexFrom = indexFrom;
            TotalCount = totalCount ?? enumerable.Count;
            TotalPages = (int)Math.Ceiling(TotalCount / (double)PageSize);

            Items = enumerable
                .Skip((PageIndex - IndexFrom) * PageSize)
                .Take(PageSize)
                .ToList();
        }
    }

    /// <summary>
    ///     Инициализирует новый пустой экземпляр класса <see cref="PagedList{T}" />.
    /// </summary>
    internal PagedList()
    {
        Items = Array.Empty<T>();
    }

    /// <inheritdoc />
    public int PageIndex { get; }

    /// <inheritdoc />
    public int PageSize { get; }

    /// <inheritdoc />
    public int TotalCount { get; }

    /// <inheritdoc />
    public int TotalPages { get; }

    /// <inheritdoc />
    public int IndexFrom { get; }

    /// <inheritdoc />
    public IList<T> Items { get; }

    /// <inheritdoc />
    public bool HasPreviousPage => PageIndex - IndexFrom > 0;

    /// <inheritdoc />
    public bool HasNextPage => PageIndex - IndexFrom + 1 < TotalPages;
}
