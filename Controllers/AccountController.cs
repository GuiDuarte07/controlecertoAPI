using Microsoft.AspNetCore.Mvc;

namespace Finantech.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
