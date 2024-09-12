using Microsoft.AspNetCore.Identity;

namespace Money.Api.Common.Extensions;

public static class IdentityResultExtensions
{
    public static Result ToResult(this IdentityResult identityResult)
    {
        return identityResult.Succeeded
            ? Result.Success()
            : Result.Failure(identityResult.Errors.Select(error => error.Description).ToArray());
    }
}
