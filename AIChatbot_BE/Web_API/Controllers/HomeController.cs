using Microsoft.AspNetCore.Mvc;

namespace Web_API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
