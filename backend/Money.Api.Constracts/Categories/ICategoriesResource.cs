using Refit;

namespace Money.Api.Constracts.Categories
{
    public interface ICategoriesResource
    {
        [Get("/Categories")]
        Task<IEnumerable<CategoryDTO>> GetListAsync(
            [Query] string? type = null,
            CancellationToken cancellationToken = default);

        [Get("/Categories/{id}")]
        Task<CategoryDTO> GetByIdAsync(
            [AliasAs("id")] int id,
            CancellationToken cancellationToken = default);

        [Post("/Categories")]
        Task<int> CreateAsync(
            CategoryDetailsDTO operation,
            CancellationToken cancellationToken = default);

        [Post("/Categories/{id}")]
        Task UpdateAsync(
            [AliasAs("id")] int id,
            CategoryDetailsDTO operation,
            CancellationToken cancellationToken = default);

        [Delete("/Categories/{id}")]
        Task DeleteAsync(
            [AliasAs("id")] int id,
            CancellationToken cancellationToken = default);

        [Post("/Categories/{id}/Restore")]
        Task RestoreAsync(
            [AliasAs("id")] int id,
            CancellationToken cancellationToken = default);

    }
}