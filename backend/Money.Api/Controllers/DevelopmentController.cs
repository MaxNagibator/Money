using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Money.Business;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Extensions;
using OpenIddict.Validation.AspNetCore;

namespace Money.Api.Controllers;

#if DEBUG

/// <summary>
/// Контроллер только для разработки.
/// </summary>
[ApiController]
[Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
[Route("[controller]")]
public class DevelopmentController(RequestEnvironment environment, ApplicationDbContext context) : ControllerBase
{
    /// <summary>
    /// Восстанавливает все данные пользователя, удаляя существующие категории, операции и места,
    /// а затем добавляя новые.
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

        var dbUser = await context.DomainUsers.SingleOrDefaultAsync(x => x.Id == environment.UserId, cancellationToken)
                     ?? throw new BusinessException("Извините, но пользователь не найден.");

        context.Categories.RemoveRange(context.Categories.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        context.Operations.RemoveRange(context.Operations.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        context.Places.RemoveRange(context.Places.IsUserEntity(environment.UserId));

        var categories = DatabaseSeeder.SeedCategories(environment.UserId.Value, out var lastIndex);
        (var operations, var places) = DatabaseSeeder.SeedOperations(environment.UserId.Value, categories);

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
