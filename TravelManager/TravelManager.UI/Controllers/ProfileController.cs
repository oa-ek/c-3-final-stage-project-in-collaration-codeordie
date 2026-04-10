using Microsoft.AspNetCore.Mvc;

namespace TravelManager.UI.Controllers
{
    public class ProfileController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
