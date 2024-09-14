namespace Money.Api.Common.Pagination;

/// <summary>
///     Предоставляет интерфейсы для постраничного списка любого типа.
/// </summary>
/// <typeparam name="T">Тип данных для постраничного отображения.</typeparam>
public interface IPagedList<T>
{
    /// <summary>
    ///     Начальный индекс.
    /// </summary>
    /// <value>Значение начального индекса.</value>
    int IndexFrom { get; }

    /// <summary>
    ///     Индекс текущей страницы.
    /// </summary>
    int PageIndex { get; }

    /// <summary>
    ///     Размер страницы.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    ///     Общее количество элементов в списке типа <typeparamref name="T" />.
    /// </summary>
    int TotalCount { get; }

    /// <summary>
    ///     Общее количество страниц.
    /// </summary>
    int TotalPages { get; }

    /// <summary>
    ///     Элементы текущей страницы.
    /// </summary>
    IList<T> Items { get; }

    /// <summary>
    ///     Признак наличия предыдущей страницы.
    /// </summary>
    /// <value>Значение, указывающее, есть ли предыдущая страница.</value>
    bool HasPreviousPage { get; }

    /// <summary>
    ///     Признак наличия следующей страницы.
    /// </summary>
    /// <value>Значение, указывающее, есть ли следующая страница.</value>
    bool HasNextPage { get; }
}
