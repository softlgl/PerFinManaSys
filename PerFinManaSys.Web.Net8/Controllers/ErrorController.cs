using Microsoft.AspNetCore.Mvc;

namespace PerFinManaSys.Web.Net8.Controllers;

public class ErrorController : Controller
{
    public IActionResult Index()
    {
        ViewBag.ErrorMessage = TempData["ErrorMessage"] as string ?? "系统发生错误";
        return View();
    }

    public IActionResult NoAuthorize()
    {
        return View();
    }
}
