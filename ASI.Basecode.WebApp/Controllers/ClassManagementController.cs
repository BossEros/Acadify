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

        ViewBag.TeachersList = teachers.Select(t => new
        {
            id = t.Id,
            name = $"{t.FirstName} {t.LastName}"
        }).ToList();
    
        return View();
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
        

        var abbreviations  = Days.Select(d => d switch
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
}