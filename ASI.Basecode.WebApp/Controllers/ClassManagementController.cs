namespace ASI.Basecode.WebApp.Controllers;

using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;


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
        var courses = await _courseManagementService.GetAllCoursesAsync();
        var allUsers = await _userManagementService.GetAllUsersAsync();
        var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

        var nextId = await _classManagementService.GetNextClassIdAsync();
        ViewBag.NextEdpCode = nextId.ToString();

        ViewBag.Courses = courses
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CourseCode
            })
            .ToList();

        ViewBag.CourseList = courses.Select(c => new
        {
            id = c.Id,
            courseName = c.CourseName,
            courseUnit = c.Units
        }).ToList();

        ViewBag.Teachers = teachers.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = $"{t.FirstName} {t.LastName}"
        }).ToList();

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var classes = await _classManagementService.GetAllClassesAsync();
        return View(classes);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var classEntity = await _classManagementService.GetClassByIdAsync(id);

        if (classEntity == null)
            return NotFound();

        return PartialView("Details", classEntity);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var classEntity = await _classManagementService.GetClassByIdAsync(id);
        if (classEntity == null)
            return NotFound();

        var courses = await _courseManagementService.GetAllCoursesAsync();
        var allUsers = await _userManagementService.GetAllUsersAsync();
        var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

        ViewBag.Courses = new SelectList(courses, "Id", "CourseCode", classEntity.CourseId);
        ViewBag.Teachers = new SelectList(teachers, "Id", "FullName", classEntity.TeacherId);

        ViewBag.CourseList = courses.Select(c => new
        {
            id = c.Id,
            courseName = c.CourseName,
            courseUnit = c.Units
        }).ToList();

        return View(classEntity);
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _classManagementService.DeleteClassAsync(id);
            TempData["SuccessMessage"] = "Class deleted successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["ErrorMessage"] = "An unexpected error occurred while deleting the class.";
        }

        return RedirectToAction(nameof(Index));
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Class classEntity, string[] Days, string StartTime, string EndTime)
    {
        if (!ModelState.IsValid)
        {
            var courses = await _courseManagementService.GetAllCoursesAsync();

            ViewBag.Courses = courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = $"{c.CourseCode} - {c.CourseName}"
            }).ToList();
            return View(classEntity);
        }


        var abbreviations = Days.Select(d => d switch
        {
            "Monday" => "M",
            "Tuesday" => "T",
            "Wednesday" => "W",
            "Thursday" => "TH",
            "Friday" => "F",
            "Saturday" => "S",
            _ => ""
        });

        if (DateTime.TryParse(StartTime, out var start) && DateTime.TryParse(EndTime, out var end))
        {
            string startFormatted = start.ToString("h:mm tt");
            string endFormatted = end.ToString("h:mm tt");

            classEntity.Schedule = $"{string.Join("", abbreviations)}, {startFormatted} – {endFormatted}";
        }
        else
        {
            classEntity.Schedule = string.Join("", abbreviations);
        }

        await _classManagementService.CreateClassAsync(classEntity);
        return RedirectToAction("Index", "ClassManagement");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Class classEntity, string[] Days, string StartTime, string EndTime)
    {
        if (!ModelState.IsValid)
        {
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.Errors.Count > 0)
                {
                    Console.WriteLine($"{key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            // Optional: also log overall state
            Console.WriteLine("ModelState invalid — form didn't pass validation.");
    
            var courses = await _courseManagementService.GetAllCoursesAsync();
            var allUsers = await _userManagementService.GetAllUsersAsync();
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

            ViewBag.Courses = courses.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CourseCode
            }).ToList();

            ViewBag.Teachers = teachers.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = $"{t.FirstName} {t.LastName}"
            }).ToList();

            return View(classEntity);
        }

        var abbreviations = Days.Select(d => d switch
        {
            "Monday" => "M",
            "Tuesday" => "T",
            "Wednesday" => "W",
            "Thursday" => "TH",
            "Friday" => "F",
            "Saturday" => "S",
            _ => ""
        });

        if (DateTime.TryParse(StartTime, out var start) && DateTime.TryParse(EndTime, out var end))
        {
            string startFormatted = start.ToString("h:mm tt");
            string endFormatted = end.ToString("h:mm tt");
            classEntity.Schedule = $"{string.Join("", abbreviations)}, {startFormatted} – {endFormatted}";
        }
        else
        {
            classEntity.Schedule = string.Join("", abbreviations);
        }

        await _classManagementService.UpdateClassAsync(classEntity);
        return RedirectToAction("Index");
    }
}