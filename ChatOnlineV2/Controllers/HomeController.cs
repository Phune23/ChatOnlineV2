using Microsoft.AspNetCore.Mvc;

namespace ChatOnlineV2.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
