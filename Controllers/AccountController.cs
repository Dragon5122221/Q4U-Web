using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Q4UPrototypeWeb.Models;
using Q4UPrototypeWeb.Services;
namespace Q4UPrototypeWeb.Controllers;
[EnableRateLimiting("login")]
public class AccountController(IdentityService identity) : Controller
{
    [HttpGet] public IActionResult Login() { ViewBag.Demo=identity.Demo; return View(); }
    [HttpGet] public IActionResult Register() { ViewBag.Demo=identity.Demo; return View(); }
    [HttpGet] public IActionResult Denied() { Response.StatusCode=403; return Content("Your profile cannot access this section. Return to /Workspace or sign in with an authorised account."); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Demo(string role="Customer")
    {
        if(!identity.Demo || !Roles.All.Contains(role)) return Forbid();
        var tenant=User.FindFirst("tenant")?.Value ?? "demo-"+Guid.NewGuid().ToString("N");
        var name=role=="Customer" ? "Ayesha Khan" : role=="FleetOwner" ? "Jordan · Fleet owner" : role=="FleetManager" ? "Thabo · Fleet manager" : role=="Administrator" ? "Q4U administrator" : "Q4U developer";
        await HttpContext.SignInAsync("Cookies",IdentityService.Principal("demo-"+role.ToLowerInvariant(),tenant,role,name));
        return RedirectToAction("Index","Workspace",new { section=role=="Developer" ? "Developer" : "Dashboard" });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Firebase(string idToken)
    {
        try { await HttpContext.SignInAsync("Cookies",await identity.Verify(idToken)); return RedirectToAction("Index","Workspace"); }
        catch { ViewBag.Demo=identity.Demo; ModelState.AddModelError("","Sign-in could not be completed. Check your credentials and active workspace membership."); return View("Login"); }
    }
    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync("Cookies"); return RedirectToAction("Login"); }
}
