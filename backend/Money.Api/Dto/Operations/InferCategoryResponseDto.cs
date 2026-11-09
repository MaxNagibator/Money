namespace Money.Api.Dto.Operations;

/// <summary>
/// Ответ с подсказкой категории по месту.
/// </summary>
public class InferCategoryResponseDto
{
    /// <summary>
    /// Идентификатор подсказанной категории.
    /// </summary>
    public required int CategoryId { get; set; }
}
