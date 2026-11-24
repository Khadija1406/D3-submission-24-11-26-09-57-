using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PulseMvc.Models;

namespace PulseMvc.Controllers;

public class HomeController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<HomeController> _logger;

    public HomeController(UserManager<ApplicationUser> userManager, ILogger<HomeController> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User) 
                       ?? await _userManager.FindByNameAsync(User.Identity.Name ?? "");
            if (user != null)
            {
                string displayName = !string.IsNullOrWhiteSpace(user.FullName)
                    ? user.FullName
                    : (!string.IsNullOrWhiteSpace(user.UserName) && !user.UserName.Contains("@")
                        ? user.UserName
                        : "Friend");
                ViewData["UserDisplayName"] = displayName;
            }
        }
        return View();
    }
}
