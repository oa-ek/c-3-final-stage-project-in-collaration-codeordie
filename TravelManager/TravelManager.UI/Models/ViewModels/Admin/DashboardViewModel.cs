namespace TravelManager.UI.Models.ViewModels.Admin
{
    public class DashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAdmins { get; set; }
        public int TotalTrips { get; set; }

        public List<string> RegistrationMonths { get; set; } = new List<string>();
        public List<int> RegistrationsCount { get; set; } = new List<int>();
        public int NormalUsersCount => TotalUsers - TotalAdmins;
    }
}