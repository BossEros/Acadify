namespace ASI.Basecode.WebApp.Controllers;

using ASI.Basecode.Data.Models;
using ASI.Basecode.Services.Interfaces;
using ASI.Basecode.WebApp.ViewModels.ClassManagement;
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
            courseCode = c.CourseCode,
            courseName = c.CourseName,
            courseUnit = c.Units,
            courseYear = c.YearLevel,
            courseSem = c.AvailableSemester
        }).ToList();

        ViewBag.Teachers = teachers.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = $"{t.FirstName} {t.LastName}"
        }).ToList();

        var viewModel = new CreateClassViewModel
        {
            Id = nextId
        };

        return PartialView("Create", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var classes = await _classManagementService.GetAllClassesAsync();
        var sortedClasses = classes.OrderBy(c => c.Id).ToList();
        return View(sortedClasses);
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

        // Parse schedule to get days and times
        var days = new List<string>();
        string startTime = "";
        string endTime = "";

        if (!string.IsNullOrEmpty(classEntity.Schedule))
        {
            var parts = classEntity.Schedule.Split(',');
            if (parts.Length > 0)
            {
                var dayAbbreviations = parts[0].Trim();
                if (dayAbbreviations.Contains("TH"))
                {
                    days.Add("Thursday");
                    dayAbbreviations = dayAbbreviations.Replace("TH", "");
                }
                if (dayAbbreviations.Contains("M")) days.Add("Monday");
                if (dayAbbreviations.Contains("T")) days.Add("Tuesday");
                if (dayAbbreviations.Contains("W")) days.Add("Wednesday");
                if (dayAbbreviations.Contains("F")) days.Add("Friday");
                if (dayAbbreviations.Contains("S")) days.Add("Saturday");
            }

            if (parts.Length > 1)
            {
                var timeRange = parts[1].Trim();
                var timeParts = timeRange.Split('–');
                if (timeParts.Length == 2)
                {
                    if (DateTime.TryParse(timeParts[0].Trim(), out var start))
                        startTime = start.ToString("HH:mm");
                    if (DateTime.TryParse(timeParts[1].Trim(), out var end))
                        endTime = end.ToString("HH:mm");
                }
            }
        }

        var viewModel = new EditClassViewModel
        {
            Id = classEntity.Id,
            CourseId = classEntity.CourseId,
            TeacherId = classEntity.TeacherId,
            Semester = classEntity.Semester,
            YearLevel = classEntity.YearLevel,
            Days = days.ToArray(),
            StartTime = startTime,
            EndTime = endTime
        };

        ViewBag.Courses = courses
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.CourseCode
            })
            .ToList();

        ViewBag.CourseInfo = courses
            .Select(c => new
            {
                Id = c.Id,
                CourseCode = c.CourseCode,
                Description = c.CourseName,
                Unit = c.Units
            })
            .FirstOrDefault();


        ViewBag.CourseList = courses.Select(c => new
        {
            id = c.Id,
            courseCode = c.CourseCode,
            courseName = c.CourseName,
            courseUnit = c.Units,
            courseYear = c.YearLevel,
            courseSem = c.AvailableSemester
        }).ToList();

        ViewBag.Teachers = teachers.Select(t => new SelectListItem
        {
            Value = t.Id.ToString(),
            Text = $"{t.FirstName} {t.LastName}"
        }).ToList();

        return PartialView("Edit", viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var classEntity = await _classManagementService.GetClassByIdAsync(id);
        if (classEntity == null)
            return NotFound();

        return PartialView(classEntity);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateClassViewModel viewModel)
    {
        // Check if end time is after start time
        if (!string.IsNullOrEmpty(viewModel.StartTime) && !string.IsNullOrEmpty(viewModel.EndTime))
        {
            if (TimeSpan.TryParse(viewModel.StartTime, out var startTime) &&
                TimeSpan.TryParse(viewModel.EndTime, out var endTime))
            {
                if (endTime <= startTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                }
            }
        }

        // Check if teacher is selected (not default value)
        if (viewModel.TeacherId == 0)
        {
            ModelState.AddModelError("TeacherId", "Teacher is required");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors = errors });
        }

        // Map ViewModel to Entity
        var classEntity = new Class
        {
            Id = viewModel.Id,
            CourseId = viewModel.CourseId,
            TeacherId = viewModel.TeacherId,
            Semester = viewModel.Semester,
            YearLevel = viewModel.YearLevel,
            Capacity = viewModel.Capacity
        };

        // Build schedule string
        var abbreviations = viewModel.Days.Select(d => d switch
        {
            "Monday" => "M",
            "Tuesday" => "T",
            "Wednesday" => "W",
            "Thursday" => "TH",
            "Friday" => "F",
            "Saturday" => "S",
            _ => ""
        });

        if (DateTime.TryParse(viewModel.StartTime, out var start) && DateTime.TryParse(viewModel.EndTime, out var end))
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

        return Json(new { success = true, message = "Class created successfully" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditClassViewModel viewModel)
    {
        // Check if end time is after start time
        if (!string.IsNullOrEmpty(viewModel.StartTime) && !string.IsNullOrEmpty(viewModel.EndTime))
        {
            if (TimeSpan.TryParse(viewModel.StartTime, out var startTime) &&
                TimeSpan.TryParse(viewModel.EndTime, out var endTime))
            {
                if (endTime <= startTime)
                {
                    ModelState.AddModelError("EndTime", "End time must be after start time");
                }
            }
        }

        // Check if teacher is selected (not default value)
        if (viewModel.TeacherId == 0)
        {
            ModelState.AddModelError("TeacherId", "Teacher is required");
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return Json(new { success = false, errors = errors });
        }

        // Map ViewModel to Entity
        var classEntity = new Class
        {
            Id = viewModel.Id,
            TeacherId = viewModel.TeacherId,
            Capacity = viewModel.Capacity
        };

        // Build schedule string
        var abbreviations = viewModel.Days.Select(d => d switch
        {
            "Monday" => "M",
            "Tuesday" => "T",
            "Wednesday" => "W",
            "Thursday" => "TH",
            "Friday" => "F",
            "Saturday" => "S",
            _ => ""
        });

        if (DateTime.TryParse(viewModel.StartTime, out var start) && DateTime.TryParse(viewModel.EndTime, out var end))
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

        return Json(new { success = true, message = "Class updated successfully" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmDelete(int id)
    {
        var success = await _classManagementService.DeleteClassAsync(id);

        if (success)
        {
            return Json(new { success = true, message = "Class deleted successfully." });
        }
        else
        {
            return Json(new { success = false, message = "Unable to delete class. It may be active or has enrolled students." });
        }
    }
}
