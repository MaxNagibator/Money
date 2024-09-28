using FluentValidation;

namespace Money.Web.Validators.Category;

public class CategoryDialogModelFluentValidator : AbstractValidator<Models.Category>
{
    
    public CategoryDialogModelFluentValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1,40);
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<Models.Category>.CreateWithOptions((Models.Category)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}