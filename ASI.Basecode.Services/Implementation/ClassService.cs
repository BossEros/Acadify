namespace ASI.Basecode.Services.Implementation;

using Microsoft.AspNetCore.Http;
using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;


public class ClassService : IClassService
{
    private readonly IClassRepository _classRepo;

    public ClassService(IClassRepository classRepo)
    {
        _classRepo = classRepo;
    }

    public async Task<List<Class>> GetAllClassesAsync()
    {
        return await _classRepo.GetAllAsync();
    }

    public async Task<Class?> GetClassByEdpAsync(int edpCode)
    {
        return await _classRepo.GetByEdpAsync(edpCode);
    }

    public async Task CreateClassAsync(Class model, HttpRequest request)
    {
        var schedule = BuildScheduleFromForm(request);
        if (!string.IsNullOrWhiteSpace(schedule))
            model.Schedule = schedule;

        model.IsActive = false;
        model.CreatedAt = DateTime.UtcNow;

        await _classRepo.AddAsync(model);
    }

    public async Task<bool> UpdateClassAsync(Class model, HttpRequest request)
    {
        var cls = await _classRepo.GetByEdpAsync(model.Id);
        if (cls == null) return false;

        cls.CourseId = model.CourseId;
        cls.Description = model.Description;
        cls.Units = model.Units;

        var schedule = BuildScheduleFromForm(request);
        cls.Schedule = string.IsNullOrWhiteSpace(schedule) ? model.Schedule : schedule;

        cls.TeacherId = model.TeacherId;
        cls.YearLevel = model.YearLevel;
        cls.Semester = model.Semester;
        cls.IsActive = model.IsActive;
        cls.Room = model.Room;

        await _classRepo.UpdateAsync(cls);
        return true;
    }

    public async Task<(bool Success, string Message)> DeleteClassAsync(int edpCode)
    {
        var cls = await _classRepo.GetByEdpAsync(edpCode);
        if (cls == null)
            return (false, "Class not found.");

        var hasStudents = cls.Enrollments != null && cls.Enrollments.Any();
        if (cls.IsActive)
            return (false, "Cannot delete: class is Active. Set to Inactive first.");
        if (hasStudents)
            return (false, "Cannot delete: there are enrolled students.");

        await _classRepo.DeleteAsync(cls);
        return (true, "Class deleted permanently.");
    }

    private static string BuildScheduleFromForm(HttpRequest request)
    {
        var days = request.Form["Days"].ToArray();
        var sh = request.Form["StartHour"].ToString();
        var sm = request.Form["StartMinute"].ToString();
        var eh = request.Form["EndHour"].ToString();
        var em = request.Form["EndMinute"].ToString();
        var mer = request.Form["Meridiem"].ToString();

        if (days.Length == 0 || string.IsNullOrWhiteSpace(sh) || string.IsNullOrWhiteSpace(eh))
            return string.Empty;

        var dayPart = string.Join("/", days);
        var start = $"{sh}:{(string.IsNullOrWhiteSpace(sm) ? "00" : sm)}";
        var end = $"{eh}:{(string.IsNullOrWhiteSpace(em) ? "00" : em)}";
        var merPart = string.IsNullOrWhiteSpace(mer) ? "" : $" {mer}";
        return $"{dayPart} {start}-{end}{merPart}".Trim();
    }
}

