using ITHelpdeshMangament.Models.Auth;
using ITHelpdeshMangament.Services;
using Microsoft.AspNetCore.Mvc;

namespace ITHelpdeshMangament.Controllers;

public class AccountController : Controller
{
    private readonly AuthApiService _authApiService;

    public AccountController(AuthApiService authApiService)
    {
        _authApiService = authApiService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _authApiService.LoginAsync(
                model,
                cancellationToken);

            if (result == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");

                return View(model);
            }

            var isAuthorized = result.Roles.Any(role =>
                role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                role.Equals("ITStaff", StringComparison.OrdinalIgnoreCase));

            if (!isAuthorized)
            {
                ModelState.AddModelError(
                    string.Empty,
                   "You do not have permission to access the admin portal.");

                return View(model);
            }

            HttpContext.Session.SetString(
                "AccessToken",
                result.AccessToken);

            HttpContext.Session.SetString(
                "Email",
                result.Email);

            HttpContext.Session.SetString(
                "Roles",
                string.Join(",", result.Roles));

            HttpContext.Session.SetString(
                "ExpiresAtUtc",
                result.ExpiresAtUtc.ToString("O"));

            return RedirectToAction("Index", "Dashboard");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
               "Unable to connect to the backend API. Please try again.");
        }
        catch (TaskCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(
                string.Empty,
               "The backend API took too long to respond. Please try again.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction("Login");
    }
}
