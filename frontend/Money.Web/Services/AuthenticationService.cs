using CSharpFunctionalExtensions;
using Money.Api.Constracts;
using Money.Api.Constracts.Accounts;
using Money.Api.Constracts.Auth;
using Money.ApiClient;

namespace Money.Web.Services;

public class AuthenticationService(
    MoneyClient client,
    AuthenticationStateProvider authStateProvider,
    AuthTokensStore tokenStore)
{
    public async Task<Result> RegisterAsync(UserDto user)
    {
        AccountRegisterInfo registerInfo = new()
        {
            Email = user.Email,
            Password = user.Password
        };
        ApiClientResponse response = await client.ResponseHandle(p => p.Accounts.RegisterAsync(registerInfo));

        if (response.IsSuccessStatusCode == false)
        {
            ProblemDetails? problemDetails = await response.GetProblemDetails();
            string error = problemDetails?.Title ?? "Ошибка регистрации аккаунта на сервере.";
            return Result.Failure(error);
        }

        return Result.Success();
    }

    public async Task<Result> LoginAsync(UserDto user)
    {
        UserAuthInfo loginInfo = new(user.Email, user.Password);

        ApiClientResponse<AuthTokens> response = await client
            .ResponseHandle(p => p.Auth.LoginAsync(loginInfo));


        Result<AuthTokens> validateResult = await ValidateAuthResponse(response);
        if (validateResult.IsFailure)
        {
            return validateResult.Map(p => p.AccessToken);
        }

        Result<AuthTokens> result = await AuthenticateAsync(response.Result);

        return await result.Tap(async () => await
            ((AuthStateProvider)authStateProvider).NotifyUserAuthentication());
    }

    public async Task<Result> LogoutAsync()
    {
        await tokenStore.Clear();
        await ((AuthStateProvider)authStateProvider).NotifyUserAuthentication();
        return Result.Success();
    }

    public async Task<Result<string>> RefreshTokenAsync()
    {
        string? token = await tokenStore.GetAccessToken();
        string? refreshToken = await tokenStore.GetRefreshToken();

        if (token == null || refreshToken == null)
        {
            return Result.Failure<string>("Не удалось загрузить токен доступа или токен обновления.");
        }

        RefreshTokenInfo refreshTokenInfo = new(refreshToken);

        ApiClientResponse<AuthTokens> response = await client
            .ResponseHandle(p => p.Auth.RefreshTokenAsync(refreshTokenInfo));

        Result<AuthTokens?> validateResult = await ValidateAuthResponse(response);
        if (validateResult.IsFailure)
        {
            return validateResult.Map(p => p.AccessToken);
        }

        Result<AuthTokens> result = await AuthenticateAsync(response.Result);
        return result.Map(data => data.AccessToken);
    }

    private async Task<Result<AuthTokens?>> ValidateAuthResponse(ApiClientResponse<AuthTokens> apiClientResponse)
    {
        if (apiClientResponse.IsSuccessStatusCode == false)
        {
            ProblemDetails? problemDetails = await apiClientResponse.GetProblemDetails();
            string error = problemDetails?.Title ?? "Не удалось получить данные авторизации.";
            return Result.Failure<AuthTokens?>(error);
        }

        return Result.Success<AuthTokens?>(apiClientResponse.Result);
    }

    private async Task<Result<AuthTokens>> AuthenticateAsync(AuthTokens? tokens)
    {
        if (tokens == null)
        {
            return Result.Failure<AuthTokens>("Некорректный ответ от сервера.");
        }

        await tokenStore.SetAccessToken(tokens.AccessToken);
        await tokenStore.SetRefreshToken(tokens.RefreshToken);
        return Result.Success(tokens);
    }
}
