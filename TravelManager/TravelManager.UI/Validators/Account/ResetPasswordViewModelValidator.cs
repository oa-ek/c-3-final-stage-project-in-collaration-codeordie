using FluentValidation;
using TravelManager.UI.Models.ViewModels.Account;

namespace TravelManager.UI.Validators.Account
{
    public class ResetPasswordViewModelValidator : AbstractValidator<ResetPasswordViewModel>
    {
        public ResetPasswordViewModelValidator()
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введіть новий пароль.")
                .MinimumLength(6).WithMessage("Пароль має містити мінімум 6 символів.");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Паролі не співпадають.");

            RuleFor(x => x.Token).NotEmpty();
        }
    }
}
