using Microsoft.AspNetCore.Mvc;

namespace TravelManager.UI.Controllers
{
    public class TripsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
