using FluentValidation;
using TravelManager.UI.Models.ViewModels.Account;

namespace TravelManager.UI.Validators.Account
{
    public class ChangePasswordViewModelValidator : AbstractValidator<ChangePasswordViewModel>
    {
        public ChangePasswordViewModelValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Введіть поточний пароль.");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Введіть новий пароль.")
                .MinimumLength(6).WithMessage("Пароль має містити мінімум 6 символів.");

            RuleFor(x => x.ConfirmNewPassword)
                .Equal(x => x.NewPassword).WithMessage("Паролі не співпадають.");
        }
    }
}