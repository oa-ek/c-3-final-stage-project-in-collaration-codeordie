namespace TravelManager.UI.Models.ViewModels.Account
{
    public class ProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty; 
        public string? PhoneNumber { get; set; }

        // Для отримання файлу з форми
        public IFormFile? ProfileImage { get; set; }
        public byte[]? CurrentProfilePicture { get; set; }
    }
}
