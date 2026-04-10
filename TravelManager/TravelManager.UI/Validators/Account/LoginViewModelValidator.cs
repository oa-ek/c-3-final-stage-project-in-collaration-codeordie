using FluentValidation;
using TravelManager.UI.Models.ViewModels.Account;

namespace TravelManager.UI.Validators.Account
{
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введіть email.")
                .EmailAddress().WithMessage("Некоректний формат email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введіть пароль.");
        }
    }
}
