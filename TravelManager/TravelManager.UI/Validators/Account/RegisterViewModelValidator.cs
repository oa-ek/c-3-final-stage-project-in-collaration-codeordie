using FluentValidation;
using TravelManager.UI.Models.ViewModels.Account;

namespace TravelManager.UI.Validators.Account
{
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("Введіть ім'я.")
                .MaximumLength(50).WithMessage("Ім'я не може перевищувати 50 символів.");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Введіть прізвище.")
                .MaximumLength(50).WithMessage("Прізвище не може перевищувати 50 символів.");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введіть email.")
                .EmailAddress().WithMessage("Некоректний формат email.");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введіть пароль.")
                .MinimumLength(6).WithMessage("Пароль має містити мінімум 6 символів.");

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty().WithMessage("Підтвердіть пароль.")
                .Equal(x => x.Password).WithMessage("Паролі не співпадають.");
        }
    }
}
