using Money.Api.Constracts.Operations;

namespace Money.Api.Extensions;

public static class OperationExtensions
{
    public static OperationDTO ToOperationDTO(this Operation business)
    {
        return new OperationDTO
        {
            Id = business.Id,
            CategoryId = business.CategoryId,
            Sum = business.Sum,
            Comment = business.Comment,
            Place = business.Place,
            Date = business.Date,
            CreatedTaskId = business.CreatedTaskId,
        };
    }

    public static Operation ToBusinessOperation(this OperationDTODetails operation)
    {
        return new Operation
        {
            CategoryId = operation.CategoryId,
            Sum = operation.Sum,
            Comment = operation.Comment,
            Place = operation.Place,
            Date = operation.Date,
        };
    }

    public static Operation ToBusinessOperation(this OperationDTODetails operation, int id)
    {
        Operation model = ToBusinessOperation(operation);
        model.Id = id;
        return model;
    }

    public static OperationFilter ToBusinessFilter(this OperationDTOFilter dtoFilter)
    {
        return new OperationFilter
        {
            CategoryIds = dtoFilter.CategoryIds,
            Comment = dtoFilter.Comment,
            Place = dtoFilter.Place,
            DateFrom = dtoFilter.DateFrom,
            DateTo = dtoFilter.DateTo,
        };
    }
}