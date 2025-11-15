using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Student_Performance_Tracker.ViewModels.Account;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Resources.Messages;
using System.Linq;

namespace Student_Performance_Tracker.Controllers;

[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // GET: /Account/Register
    [HttpGet]
    public ViewResult Register() => View();

    // GET: /Account/Login
    [HttpGet]
    public ViewResult Login(string? returnUrl = null)
    {
        ViewData[nameof(returnUrl)] = returnUrl;
        return View();
    }

    // GET: /Account/ForgotPassword
    [HttpGet]
    public ViewResult ForgotPassword() => View();

    // GET: /Account/ResetPassword
    [HttpGet]
    public IActionResult ResetPassword(string email, string token)
    {
        // Return to login page if one is missing to prevent tampering
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewModel{Email = email, Token = token});
    }

    // GET: /Account/ConfirmEmail
    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string email, string token)
    {
        // Return to login page if one is missing to prevent tampering
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        try
        {
            var result = await _accountService.ConfirmEmailAsync(email, token);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Email verified successfully! You can now sign in with your credentials.";
                return View("EmailConfirmed");
            }

            // If confirmation failed
            TempData["ErrorMessage"] = result.Errors.FirstOrDefault() ?? "Email verification failed. The link may have expired or is invalid.";
            return RedirectToAction("Login");
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An unexpected error occurred during email verification. Please try again.";
            return RedirectToAction("Login");
        }
    }

    // GET: /Account/RegisterConfirmation
    [HttpGet]
    public ViewResult RegisterConfirmation() => View();


    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _accountService.RegisterAsync(new RegisterRequest
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                Role = model.Role
            });

            if (result.Succeeded)
            {
                return RedirectToAction("RegisterConfirmation", "Account");
            }

            // if registation is not successful, add errors to ModelState
            foreach (var error in result.Errors)
            {
                if (error.Contains("Email"))
                {
                    ModelState.AddModelError(nameof(model.Email), error);
                }
                else
                {
                    ModelState.AddModelError("", error);
                }
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An unexpected error occurred during registration. Please try again.");
        }

        return View(model);
    }

    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        if (!ModelState.IsValid)
        {
            return ViewWithReturnUrl(model, returnUrl);
        }

        try
        {
            var result = await _accountService.LoginAsync(new LoginRequest
            {
                Email = model.Email,
                Password = model.Password
            });

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return await RedirectBasedOnRoleAsync(model.Email);
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError("", AccountMessages.AccountLockedOut);
                return ViewWithReturnUrl(model, returnUrl);
            }
            
            ModelState.AddModelError("", result.ErrorMessage ?? AccountMessages.InvalidLoginAttempt);
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An unexpected error occurred during login. Please try again.");
        }

        return ViewWithReturnUrl(model, returnUrl);
    }

    // POST: /Account/ForgotPassword
    [HttpPost] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.SendPasswordResetTokenAsync(model.Email);

        return View("ForgotPasswordConfirmation");
    }

    // POST: /Account/ResetPassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var result = await _accountService.ResetPasswordAsync(model.Email, model.Token, model.Password);

            if (result.Succeeded)
            {
                ViewBag.Message = AccountMessages.PasswordResetSuccessful;
                return View("ResetPasswordConfirmation");
            }

            foreach (var error in result.Errors)
            {
                // Check if token expired
                if (error.Contains("Invalid token") || error.Contains("invalid"))
                {
                    ModelState.AddModelError("", "The password reset link has expired or is invalid. Please request a new password + reset link.");
                }
                else
                {
                    ModelState.AddModelError("", error);
                }      
            }
        }
        catch (Exception)
        {
            ModelState.AddModelError("", "An unexpected error occurred during password reset. Please try again.");
        }

        return View(model);
    }

    // POST: /Auth/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }


    // Helper Methods

    private ViewResult ViewWithReturnUrl<T>(T model, string? returnUrl)
    {
        ViewData[nameof(returnUrl)] = returnUrl;
        return View(model);
    }

    private async Task<IActionResult> RedirectBasedOnRoleAsync(string email)
    {
        var redirectPath = await _accountService.GetRedirectPathBasedOnRoleAsync(email);
        return Redirect(redirectPath);
    }
}