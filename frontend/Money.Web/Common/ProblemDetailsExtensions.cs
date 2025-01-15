using Money.ApiClient;

namespace Money.Web.Common;

public static class ProblemDetailsExtensions
{
    public static ProblemDetails? ShowMessage(this ProblemDetails? problemDetails, ISnackbar snackbar)
    {
        if (problemDetails == null)
        {
            return null;
        }

        snackbar.Add(problemDetails.Title, Severity.Warning);
        return problemDetails;
    }

    public static bool HasError(this ProblemDetails? problemDetails)
    {
        return problemDetails != null;
    }

    public static bool IsBadRequest(this ApiClientResponse response, ISnackbar snackbar)
    {
        return response.GetError().ShowMessage(snackbar).HasError();
    }
}
