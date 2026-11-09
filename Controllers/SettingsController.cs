using Microsoft.AspNetCore.Mvc;
using Q4UPrototypeWeb.Models;

namespace Q4UPrototypeWeb.Controllers
{
    public class SettingsController : Controller
    {
        public IActionResult Index()
        {
            var model =
                new SettingsViewModel();

            return View(model);
        }
    }
}