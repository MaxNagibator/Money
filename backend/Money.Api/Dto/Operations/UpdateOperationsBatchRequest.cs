namespace Money.Api.Dto.Operations;

/// <summary>
/// Запрос на обновление пакета операций.
/// </summary>
public class UpdateOperationsBatchRequest
{
    /// <summary>
    /// Список идентификаторов, которые необходимо обновить.
    /// </summary>
    public required List<int> OperationIds { get; init; }

    /// <summary>
    /// Идентификатор категории, в которую необходимо переместить операции.
    /// </summary>
    public required int CategoryId { get; init; }
}
