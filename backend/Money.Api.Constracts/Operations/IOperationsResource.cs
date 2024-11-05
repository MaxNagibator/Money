using Refit;

namespace Money.Api.Constracts.Operations
{
    public interface IOperationsResource
    {
        [Get("/Operations")]
        Task<IEnumerable<OperationDTO>> GetListAsync(
            [Query] OperationDTOFilter filters,
            CancellationToken cancellationToken = default);

        [Get("/Operations/{id}")]
        Task<OperationDTO> GetByIdAsync(
            [AliasAs("id")] int id,
            CancellationToken cancellationToken = default);

        [Post("/Operations")]
        Task<int> CreateAsync(
            OperationDTODetails operation,
            CancellationToken cancellationToken = default);

        [Post("/Operations/{id}")]
        Task UpdateAsync(
            [AliasAs("id")] int id,
            OperationDTODetails operation,
            CancellationToken cancellationToken = default);

        [Delete("/Operations/{id}")]
        Task DeleteAsync(
            [AliasAs("id")] int id,
            CancellationToken cancellationToken = default);

        [Post("/Operations/{id}/Restore")]
        Task RestoreAsync(
            [AliasAs("id")] int id,
            CancellationToken cancellationToken = default);

        [Post("/Operations/GetPlaces/{offset}/{count}")]
        Task<string[]> GetPlacesAsync(
            [AliasAs("offset")] int offset,
            [AliasAs("count")] int count,
            CancellationToken cancellationToken = default);

        [Post("/Operations/GetPlaces/{offset}/{count}/{name}")]
        Task<string[]> GetPlacesAsync(
            [AliasAs("offset")] int offset,
            [AliasAs("count")] int count,
            [AliasAs("name")] string name,
            CancellationToken cancellationToken = default);
    }
}