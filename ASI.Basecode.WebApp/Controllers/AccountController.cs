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
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
        {
            return RedirectToAction("Login");
        }

        var model = new ResetPasswordViewModel{Email = email, Token = token};
        return View(model);
    }


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
                return RedirectToAction("Login", "Account");
            }

            // Add errors to ModelState
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
            // Log the exception (add logging service)
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
                Password = model.Password,
                RememberMe = model.RememberMe
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
            // Log the exception (add logging service)
            ModelState.AddModelError("", "An unexpected error occurred during login. Please try again.");
        }

        return ViewWithReturnUrl(model, returnUrl);
    }

    // POST: /Account/ForgotPassword
    [HttpPost] 
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
                ModelState.AddModelError("", error);
            }
        }
        catch (Exception)
        {
            // Log the exception (add logging service)
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
        return RedirectToAction("Index", "Home");
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