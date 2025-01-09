namespace Money.Api.Dto.Debts;

/// <summary>
/// Простить долг.
/// </summary>
public class ForgiveRequest
{
    /// <summary>
    /// Идентификаторы долгов.
    /// </summary>
    public required int[] DebtIds { get; init; }

    /// <summary>
    /// Идентификатор категории расходов.
    /// </summary>
    public int OperationCategoryId { get; init; }

    /// <summary>
    /// Комментарий в расходах.
    /// </summary>
    public string? OperationComment { get; set; }
}
