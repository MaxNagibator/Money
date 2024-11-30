using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Money.Business;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;
using OpenIddict.Validation.AspNetCore;
using Category = Money.Data.Entities.Category;
using Operation = Money.Data.Entities.Operation;
using Place = Money.Data.Entities.Place;

namespace Money.Api.Controllers;

#if DEBUG

/// <summary>
///     Контроллер только для разработки.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class DevelopmentController(RequestEnvironment environment, ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    ///     Восстанавливает все данные пользователя, удаляя существующие категории, операции и места,
    ///     а затем добавляя новые.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    [HttpPost]
    [Route("/LoadTestData")]
    public async Task LoadTestData(CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        DomainUser dbUser = await context.DomainUsers.SingleOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Извините, но пользователь не найден.");

        context.Categories.RemoveRange(context.Categories.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        context.Operations.RemoveRange(context.Operations.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        context.Places.RemoveRange(context.Places.IsUserEntity(environment.UserId));

        List<Category> categories = DatabaseSeeder.SeedCategories(environment.UserId.Value, out int lastIndex);
        (List<Operation> operations, List<Place> places) = DatabaseSeeder.SeedOperations(environment.UserId.Value, categories);

        dbUser.NextCategoryId = lastIndex + 1;
        dbUser.NextPlaceId = places.Last().Id + 1;
        dbUser.NextOperationId = operations.Last().Id + 1;

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.Places.AddRangeAsync(places, cancellationToken);
        await context.Operations.AddRangeAsync(operations, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}

#endif
