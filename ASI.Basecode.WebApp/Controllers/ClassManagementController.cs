using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;

namespace Student_Performance_Tracker.Controllers
{
    [Authorize]
    public class ClassManagementController : Controller
    {
        private readonly IClassService _classService;

        public ClassManagementController(IClassService classService)
        {
            _classService = classService;
        }

        // ===== VIEW / INDEX =====
        public async Task<IActionResult> Index()
        {
            var classes = await _classService.GetAllClassesAsync();
            return View(classes);
        }

        // ===== CREATE =====
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(new Class());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Class model)
        {
            if (!ModelState.IsValid)
                return View(model);

            await _classService.CreateClassAsync(model, Request);
            TempData["Message"] = "Class created successfully (set as Inactive by default).";
            TempData["MessageType"] = "success";
            return RedirectToAction(nameof(Index));
        }

        // ===== EDIT =====
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditLookup(int edpCode)
        {
            var cls = await _classService.GetClassByEdpAsync(edpCode);
            if (cls == null)
            {
                TempData["Message"] = "Class not found.";
                TempData["MessageType"] = "error";
                return View("Edit", new Class { Id = edpCode });
            }
            return View("Edit", cls);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var cls = await _classService.GetClassByEdpAsync(id);
            if (cls == null)
            {
                TempData["Message"] = "Class not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
            return View(cls);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Class model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _classService.UpdateClassAsync(model, Request);

            TempData["Message"] = result
                ? "Class updated successfully."
                : "Class not found.";
            TempData["MessageType"] = result ? "success" : "error";
            return RedirectToAction(nameof(Index));
        }

        // ===== DELETE =====
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLookup(int edpCode)
        {
            var cls = await _classService.GetClassByEdpAsync(edpCode);
            if (cls == null)
            {
                TempData["Message"] = "Class not found.";
                TempData["MessageType"] = "error";
                return View("Delete", new Class { Id = edpCode });
            }
            return View("Delete", cls);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var cls = await _classService.GetClassByEdpAsync(id);
            if (cls == null)
            {
                TempData["Message"] = "Class not found.";
                TempData["MessageType"] = "error";
                return RedirectToAction(nameof(Index));
            }
            return View(cls);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int edpCode)
        {
            var result = await _classService.DeleteClassAsync(edpCode);

            TempData["Message"] = result.Success ? result.Message : "Unable to delete class.";
            TempData["MessageType"] = result.Success ? "success" : "error";
            return RedirectToAction(nameof(Index));
        }
    }
}
