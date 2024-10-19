using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Money.Business;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Entities;
using Money.Data.Extensions;
using OpenIddict.Validation.AspNetCore;

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
    ///     Восстанавливает все данные пользователя, удаляя существующие категории, платежи и места,
    ///     а затем добавляя новые.
    /// </summary>
    /// <param name="cancellationToken">Токен отмены операции.</param>
    [HttpPost]
    [Route("/Restore")]
    public async Task RestoreAllAsync(CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken)
                            ?? throw new BusinessException("Извините, но пользователь не найден.");

        context.Categories.RemoveRange(context.Categories.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        context.Payments.RemoveRange(context.Payments.IgnoreQueryFilters().IsUserEntity(environment.UserId));
        context.Places.RemoveRange(context.Places.IsUserEntity(environment.UserId));

        List<DomainCategory> categories = DatabaseSeeder.SeedCategories(environment.UserId.Value);
        (List<DomainPayment> payments, List<DomainPlace> places) = DatabaseSeeder.SeedPayments(environment.UserId.Value);

        dbUser.NextCategoryId = categories.Last().Id + 1;
        dbUser.NextPlaceId = places.Last().Id + 1;
        dbUser.NextPaymentId = payments.Last().Id + 1;

        await context.Categories.AddRangeAsync(categories, cancellationToken);
        await context.Places.AddRangeAsync(places, cancellationToken);
        await context.Payments.AddRangeAsync(payments, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }
}

#endif
