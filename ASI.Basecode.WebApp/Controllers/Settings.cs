using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(IAccountService accountService, ILogger<SettingsController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUsername))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = await _accountService.GetCurrentUserAsync(currentUsername);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userDetails = new UserDetailsViewModel
            {
                Name = $"{user.FirstName} {user.LastName}",
                UserId = user.Id.ToString(),
                Email = user.Email ?? string.Empty
            };

            var viewModel = new SettingsViewModel
            {
                UserDetails = userDetails
            };

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var currentUsername = User.Identity?.Name;
                if (string.IsNullOrEmpty(currentUsername))
                {
                    return RedirectToAction("Login", "Account");
                }

                var user = await _accountService.GetCurrentUserAsync(currentUsername);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                var userDetails = new UserDetailsViewModel
                {
                    Name = $"{user.FirstName} {user.LastName}",
                    UserId = user.Id.ToString(),
                    Email = user.Email ?? string.Empty
                };

                var settingsViewModel = new SettingsViewModel
                {
                    UserDetails = userDetails,
                    ChangePasswordModel = model
                };

                return View("Index", settingsViewModel);
            }

            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _accountService.ChangePasswordAsync(currentUser, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Password changed successfully!";
                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors ?? Enumerable.Empty<string>())
            {
                ModelState.AddModelError(string.Empty, error);
            }

            // Return to settings with errors
            var currentUserData = await _accountService.GetCurrentUserAsync(currentUser);
            if (currentUserData == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var userDetailsWithErrors = new UserDetailsViewModel
            {
                Name = $"{currentUserData.FirstName} {currentUserData.LastName}",
                UserId = currentUserData.Id.ToString(),
                Email = currentUserData.Email ?? string.Empty
            };

            return View("Index", new SettingsViewModel
            {
                UserDetails = userDetailsWithErrors,
                ChangePasswordModel = model
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAccount()
        {
            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _accountService.DeleteAccountAsync(currentUser);

            if (result.Succeeded)
            {
                // Sign out the user after account deletion
                await _accountService.SignOutAsync();

                TempData["InfoMessage"] = "Your account has been deleted successfully.";
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = string.Join(", ", result.Errors ?? new[] { "Unknown error occurred." });
            return RedirectToAction(nameof(Index));
        }

        // Add Logout method to match your AccountController pattern
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.SignOutAsync();
            return RedirectToAction("Login", "Account");
        }
    }
}