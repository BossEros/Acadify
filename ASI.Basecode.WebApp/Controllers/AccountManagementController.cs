using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.Services.DTOs;
using ASI.Basecode.WebApp.ViewModels.AccountManagement;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ASI.Basecode.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AccountManagementController : Controller
    {
        private readonly IUserManagementService _userManagementService;

        public AccountManagementController(IUserManagementService userManagementService)
        {
            _userManagementService = userManagementService;
        }

        // READ: View all users from database
        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userManagementService.GetAllUsersAsync();
                
                var viewModel = new UserListViewModel
                {
                    Users = users,
                    Message = TempData["Message"]?.ToString(),
                    MessageType = TempData["MessageType"]?.ToString()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var viewModel = new UserListViewModel
                {
                    Users = new List<UserManagementDto>(),
                    Message = "Error loading users: " + ex.Message,
                    MessageType = "error"
                };
                return View(viewModel);
            }
        }

        // READ: View individual user details
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);

                if (user == null)
                {
                    TempData["Message"] = "User not found.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }

                return View(user);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading user details: " + ex.Message;
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }
        }

        // CREATE: Show create form
        public async Task<IActionResult> Create()
        {
            var availableRoles = await _userManagementService.GetAvailableRolesAsync();
            var viewModel = new CreateUserViewModel
            {
                AvailableRoles = availableRoles.ToList()
            };
            return View(viewModel);
        }

        // CREATE: Add new users to the database
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new CreateUserRequest
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Password = model.Password,
                        Role = model.Role,
                        IsActive = model.IsActive
                    };

                    var result = await _userManagementService.CreateUserAsync(request);

                    if (result.Succeeded)
                    {
                        TempData["Message"] = result.Message ?? "User created successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index");
                    }

                    // Add errors to ModelState
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error creating user: " + ex.Message);
                }
            }

            // Reload available roles for the form
            model.AvailableRoles = (await _userManagementService.GetAvailableRolesAsync()).ToList();
            return View(model);
        }

        // UPDATE: Show edit form (Admin can only edit Username and IsActive)
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Message"] = "User not found.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }

                var model = new EditUserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email, // Read-only display
                    FirstName = user.FirstName, // Read-only display
                    LastName = user.LastName, // Read-only display
                    Role = user.Role, // Read-only display
                    IsActive = user.IsActive
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading user: " + ex.Message;
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }
        }

        // UPDATE: Update Username and IsActive only (Email, FirstName, LastName, and Role are read-only)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                TempData["Message"] = "Invalid user ID.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var request = new UpdateUserRequest
                    {
                        Id = model.Id,
                        UserName = model.UserName,
                        IsActive = model.IsActive
                    };

                    var result = await _userManagementService.UpdateUserAsync(request);

                    if (result.Succeeded)
                    {
                        TempData["Message"] = result.Message ?? "User updated successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error updating user: " + ex.Message);
                }
            }
            return View(model);
        }

        // CHANGE PASSWORD: Show change password form
        public async Task<IActionResult> ChangePassword(int id)
        {
            try
            {
                var user = await _userManagementService.GetUserByIdAsync(id);
                if (user == null)
                {
                    TempData["Message"] = "User not found.";
                    TempData["MessageType"] = "error";
                    return RedirectToAction("Index");
                }

                var model = new ChangePasswordViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error loading user: " + ex.Message;
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }
        }

        // CHANGE PASSWORD: Admin can change user password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var request = new ChangePasswordRequest
                    {
                        UserId = model.UserId,
                        NewPassword = model.NewPassword
                    };

                    var result = await _userManagementService.ChangePasswordAsync(request);

                    if (result.Succeeded)
                    {
                        TempData["Message"] = result.Message ?? "Password changed successfully!";
                        TempData["MessageType"] = "success";
                        return RedirectToAction("Index");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(string.Empty, "Error changing password: " + ex.Message);
                }
            }
            return View(model);
        }

        // DELETE: Show delete confirmation
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            if (user == null)
            {
                TempData["Message"] = "User not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // DELETE: Confirm delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var result = await _userManagementService.DeleteUserAsync(id);
                
                TempData["Message"] = result.Message ?? (result.Succeeded ? "User deleted successfully!" : "Error deleting user.");
                TempData["MessageType"] = result.Succeeded ? "success" : "error";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Error deleting user: " + ex.Message;
                TempData["MessageType"] = "error";
            }

            return RedirectToAction("Index");
        }
    }
}