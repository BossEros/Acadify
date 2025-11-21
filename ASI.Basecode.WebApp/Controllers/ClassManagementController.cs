namespace ASI.Basecode.WebApp.Controllers;

using ASI.Basecode.Services.DTOs;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class ClassManagementController : Controller
{
    

    private readonly IClassManagementService _classManagementService;
    private readonly ICourseManagementService _courseManagementService;
    private readonly IAccountManagementService _userManagementService;

    public ClassManagementController(IClassManagementService classManagementService, ICourseManagementService courseManagementService, IAccountManagementService userManagementService)
    {
        _classManagementService = classManagementService;
        _courseManagementService = courseManagementService;
        _userManagementService = userManagementService;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var viewModel = await _classManagementService.GetClassCreateModelAsync();

        return PartialView("Create", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var displayDtos = await _classManagementService.GetAllClassesAsync();
        return View(displayDtos);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var detailsDTO = await _classManagementService.GetClassByIdAsync(id);

        if (detailsDTO == null)
            return NotFound();

        return PartialView("Details", detailsDTO);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var viewModel = await _classManagementService.GetClassEditModelAsync(id);


        if (viewModel == null)
        return NotFound();

        return PartialView("Edit", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var viewModel = await _classManagementService.GetClassDeleteModelAsync(id);
        
        if (viewModel == null)
            return NotFound();

        return PartialView("Delete", viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClassCreateCommandDTO model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors = errors });
        }
        
        var result = await _classManagementService.CreateClassAsync(model);

        if (result.Success)
        {
            return Json(new { success = true, message = result.Message });
        }
        else
        {
            var errorsToReturn = result.Errors.Any() ? result.Errors : new List<string> { result.Message };
            return Json(new { success = false, errors = errorsToReturn });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ClassEditCommandDTO model)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return Json(new { success = false, errors = errors });
        }

        var result = await _classManagementService.UpdateClassAsync(model);

        if (result.Success)
        {
            return Json(new { success = true, message = result.Message });
        }
        else
        {
            var errorsToReturn = result.Errors.Any() ? result.Errors : new List<string> { result.Message };
            
            return Json(new { success = false, errors = errorsToReturn });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var result = await _classManagementService.DeleteClassAsync(id);

        if (result.Success)
        {
            return Json(new { success = true, message = result.Message });
        }
        else
        {
            return Json(new { success = false, message = result.Message });
        }
    }
}
