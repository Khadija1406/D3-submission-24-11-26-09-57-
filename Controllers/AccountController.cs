using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PulseMvc.Models;
using PulseMvc.Models.ViewModels;

namespace PulseMvc.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILogger<AccountController> _logger;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILogger<AccountController> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // 1. Password character count validation: Don't let enter if less than 8 characters!
        if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
        {
            ModelState.AddModelError(nameof(model.Password), "Password must be at least 8 characters long.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var identifier = model.Email.Trim();

        // 2. Account existence check: Only let enter those who have an account!
        var user = await _userManager.FindByEmailAsync(identifier) 
                   ?? await _userManager.FindByNameAsync(identifier);
        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "No account was found with that email or username. Please sign up first.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            return LocalRedirect(returnUrl ?? "/#dashboard");
        }

        ModelState.AddModelError(string.Empty, "Incorrect password. Please verify your password and try again.");
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LoginAjax([FromBody] LoginViewModel model)
    {
        // 1. Password minimum 8 characters validation
        if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
        {
            return Json(new { success = false, message = "Password must be at least 8 characters long." });
        }

        if (string.IsNullOrWhiteSpace(model.Email))
        {
            return Json(new { success = false, message = "Please enter your username or email address." });
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        var identifier = model.Email.Trim();

        // 2. Account existence check: Only let enter those who have an account!
        var user = await _userManager.FindByEmailAsync(identifier) 
                   ?? await _userManager.FindByNameAsync(identifier);
        if (user == null)
        {
            return Json(new { success = false, message = "No account found with this email or username. Please sign up first." });
        }

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: false);
        if (result.Succeeded)
        {
            // Exact username / full name set by user: NEVER strip email which may contain numbers
            string displayName = !string.IsNullOrWhiteSpace(user.FullName)
                ? user.FullName
                : (!string.IsNullOrWhiteSpace(user.UserName) && !user.UserName.Contains("@")
                    ? user.UserName
                    : "Friend");

            return Json(new
            {
                success = true,
                message = $"Welcome back, {displayName}!",
                user = new
                {
                    id = user.Id,
                    name = displayName,
                    email = user.Email
                }
            });
        }

        return Json(new { success = false, message = "Incorrect password. Please verify your password and try again." });
    }

    [HttpGet]
    public IActionResult Register(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        // 1. Password length validation
        if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
        {
            ModelState.AddModelError(nameof(model.Password), "Password must be at least 8 characters long.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var cleanEmail = model.Email.Trim();
        var cleanName = model.Name.Trim();

        var existingUser = await _userManager.FindByEmailAsync(cleanEmail);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "An account with this email already exists.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = cleanName,
            Email = cleanEmail,
            FullName = cleanName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);
            return LocalRedirect(returnUrl ?? "/#dashboard");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAjax([FromBody] RegisterViewModel model)
    {
        // 1. Enforce password minimum 8 characters validation
        if (string.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 8)
        {
            return Json(new { success = false, message = "Password must be at least 8 characters long." });
        }

        if (!ModelState.IsValid)
        {
            var errors = string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
            return Json(new { success = false, message = errors });
        }

        var cleanEmail = model.Email.Trim();
        var cleanName = model.Name.Trim();

        var existingUser = await _userManager.FindByEmailAsync(cleanEmail);
        if (existingUser != null)
        {
            return Json(new { success = false, message = "An account with this email address already exists." });
        }

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid().ToString("D"),
            UserName = cleanName,
            NormalizedUserName = cleanName.ToUpperInvariant(),
            Email = cleanEmail,
            NormalizedEmail = cleanEmail.ToUpperInvariant(),
            FullName = cleanName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(user, isPersistent: false);

            string displayName = !string.IsNullOrWhiteSpace(user.FullName) && !user.FullName.Contains("@")
                ? user.FullName
                : cleanName;

            return Json(new
            {
                success = true,
                message = $"Welcome to Pulse, {displayName}! Your workspace is ready.",
                user = new
                {
                    id = user.Id,
                    name = displayName,
                    email = user.Email
                }
            });
        }

        var errorMsg = string.Join(", ", result.Errors.Select(e => e.Description));
        return Json(new { success = false, message = errorMsg });
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> LogoutAjax()
    {
        await _signInManager.SignOutAsync();
        return Json(new { success = true, message = "Logged out successfully." });
    }

    [HttpGet]
    public async Task<IActionResult> CurrentUser()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User)
                       ?? await _userManager.FindByNameAsync(User.Identity.Name ?? "");
            if (user != null)
            {
                // Return exact name set by user: NEVER fallback to email.split('@')[0] with numbers
                string displayName = !string.IsNullOrWhiteSpace(user.FullName)
                    ? user.FullName
                    : (!string.IsNullOrWhiteSpace(user.UserName) && !user.UserName.Contains("@")
                        ? user.UserName
                        : "Friend");

                return Json(new
                {
                    isAuthenticated = true,
                    user = new
                    {
                        id = user.Id,
                        name = displayName,
                        email = user.Email
                    }
                });
            }
        }

        return Json(new { isAuthenticated = false });
    }
}
