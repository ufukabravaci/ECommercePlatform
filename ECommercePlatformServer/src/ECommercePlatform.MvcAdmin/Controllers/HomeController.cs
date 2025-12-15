using Microsoft.AspNetCore.Mvc;

namespace ECommercePlatform.MvcAdmin.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var token = HttpContext.Session.GetString("AccessToken");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login", "Auth");
        }

        ViewBag.UserName = HttpContext.Session.GetString("UserName");
        ViewBag.CompanyId = HttpContext.Session.GetString("CompanyId") ?? "Yok (Admin)";

        return View();
    }
}
