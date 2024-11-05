using Money.Api.Constracts;

namespace Money.Web.Common;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails? ShowMessage(this ProblemDetails? problemDetails, ISnackbar snackbar)
    {
        if (problemDetails == null)
        {
            return null;
        }

        _ = snackbar.Add(problemDetails.Title, Severity.Error);
        return problemDetails;
    }

    public static bool HasError(this ProblemDetails? problemDetails)
    {
        return problemDetails != null;
    }
}
