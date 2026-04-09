using FluentValidation;
using TravelManager.UI.Models.ViewModels.Account;

namespace TravelManager.UI.Validators.Account
{
    public class ProfileViewModelValidator : AbstractValidator<ProfileViewModel>
    {
        public ProfileViewModelValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty().WithMessage("Введіть ваше ім'я.");
            RuleFor(x => x.LastName).NotEmpty().WithMessage("Введіть ваше прізвище.");
        }
    }
}
