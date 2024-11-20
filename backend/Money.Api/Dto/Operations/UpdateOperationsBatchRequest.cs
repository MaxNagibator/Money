namespace Money.Api.Dto.Operations;

public class UpdateOperationsBatchRequest
{
    public required List<int> OperationIds { get; init; }
    public required int CategoryId { get; init; }
}
