using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Money.Web.Components.Account.Pages;

public partial class Login
{
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = default!;

    [SupplyParameterFromForm]
    private InputModel Input { get; set; } = new();

    [SupplyParameterFromQuery]
    private string? ReturnUrl { get; set; } = null;

    [Inject]
    private AuthenticationService AuthenticationService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    public async Task LoginUser()
    {
        await AuthenticationService.LoginAsync(new UserDto(Input.Email, Input.Password));
        NavigationManager.NavigateTo(ReturnUrl ?? string.Empty);
    }

    private sealed class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";
    }
}
