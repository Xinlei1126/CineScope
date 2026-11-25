using CineScope.Models;
using CineScope.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CineScope.Controllers;
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet] public IActionResult Register(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var user = new ApplicationUser { UserName = model.Email, Email = model.Email, DisplayName = model.DisplayName.Trim() };
        var result = await users.CreateAsync(user, model.Password);
        if (result.Succeeded) { await users.AddToRoleAsync(user, "Member"); await signIn.SignInAsync(user, false); return SafeRedirect(returnUrl); }
        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
        return View(model);
    }
    [HttpGet] public IActionResult Login(string? returnUrl = null) { ViewData["ReturnUrl"] = returnUrl; return View(); }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        if (!ModelState.IsValid) return View(model);
        var result = await signIn.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);
        if (result.Succeeded) return SafeRedirect(returnUrl);
        ModelState.AddModelError(string.Empty, result.IsLockedOut ? "Your account is temporarily locked. Try again later." : "Invalid email or password.");
        return View(model);
    }
    [HttpPost, ValidateAntiForgeryToken] public async Task<IActionResult> Logout() { await signIn.SignOutAsync(); return RedirectToAction("Index", "Home"); }
    public IActionResult AccessDenied() => View();
    private IActionResult SafeRedirect(string? url) => !string.IsNullOrEmpty(url) && Url.IsLocalUrl(url) ? LocalRedirect(url) : RedirectToAction("Index", "Home");
}
