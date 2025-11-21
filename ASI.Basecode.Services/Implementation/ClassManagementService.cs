using ASI.Basecode.Data.Models;
using ASI.Basecode.Data.Repositories;
using ASI.Basecode.Services.Interfaces;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASI.Basecode.Services.DTOs;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace ASI.Basecode.Services.Implementation
{
    public class ClassManagementService : IClassManagementService
    {
        private readonly IClassManagementRepository _classManagementRepository;
        private readonly ICourseManagementRepository _courseManagementRepository;
        private readonly IAccountManagementService _userManagementRepository;

        public ClassManagementService(IClassManagementRepository classManagementRepository, ICourseManagementRepository courseManagementRepository, IAccountManagementService userManagementRepository)
        {
            _classManagementRepository = classManagementRepository;
            _courseManagementRepository = courseManagementRepository;
            _userManagementRepository = userManagementRepository;
        }

        public async Task<IEnumerable<ClassIndexDTO>> GetAllClassesAsync()
        {   
            var classes = await _classManagementRepository.GetAllClassAsync();
            var sortedClasses = classes.OrderBy(c => c.Id).ToList();

            var displayDtos = sortedClasses
                .Select(c => new ClassIndexDTO
                {
                    EDPCode = c.Id,
                    CourseCode = c.Course.CourseCode,
                    Description = c.Course.CourseName,
                    Units = c.Course.Units,
                     Schedule = c.Schedule,
                    TeacherName = $"{c.Teacher.FirstName} {c.Teacher.LastName}",
                    Status = c.IsActive,
                    Capacity = c.Capacity,

                    // Temporary dependent variables
                    TeacherId = c.TeacherId,
                    Enrollments = c.Enrollments
                })
                .ToList();

            return displayDtos;
        }

        public async Task<ClassDetailsDTO?> GetClassByIdAsync(int id)
        {
            var classEntity = await _classManagementRepository.GetByIdAsync(id);

            if (classEntity == null)
            {
                return null; 
            }

            var studentDtos = classEntity.Enrollments?
                .Select(e => new EnrolledStudentDTO
                {
                    StudentId = e.StudentId,
                    FirstName = e.Student.FirstName,
                    LastName = e.Student.LastName,
                    MidtermGrade = e.Grade?.MidtermGrade,
                    FinalGrade = e.Grade?.FinalGrade,
                    Remarks = e.Grade?.Remarks
                })
                .ToList();
                

            var detailsDto = new ClassDetailsDTO
            {
                EDPCode = classEntity.Id,
                CourseCode = classEntity.Course.CourseCode,
                Description = classEntity.Course.CourseName,
                Units = classEntity.Course.Units,
                Semester = classEntity.Semester,
                YearLevel = classEntity.YearLevel,
                Schedule = classEntity.Schedule,
                Status = classEntity.IsActive,
                Capacity = classEntity.Capacity,
                DateCreated = classEntity.CreatedAt,
                TeacherName = $"{classEntity.Teacher.FirstName} {classEntity.Teacher.LastName}",
                
                Enrollments = studentDtos
            };

            return detailsDto;
        }

        public async Task<ClassEditDTO> GetClassEditModelAsync(int id)
        {
            var classEntity = await _classManagementRepository.GetByIdAsync(id);
            if (classEntity == null) return null;

            var courses = await _courseManagementRepository.GetAllAsync();
            var allUsers = await _userManagementRepository.GetAllUsersAsync();
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

            var scheduleData = ParseSchedule(classEntity.Schedule);

            var dto = new ClassEditDTO
            {
                EDPCode = classEntity.Id,
                CourseCode = classEntity.Course.CourseCode,
                TeacherName = $"{classEntity.Teacher.FirstName} {classEntity.Teacher.LastName}",
                Semester = classEntity.Semester,
                YearLevel = classEntity.YearLevel,
                Status = classEntity.IsActive,
                Capacity = classEntity.Capacity,

                SelectedDays = scheduleData.Days,
                StartTime = scheduleData.StartTime,
                EndTime = scheduleData.EndTime,

                CourseOptions = courses.Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.CourseCode,
                    Selected = c.Id == classEntity.CourseId 
                }).ToList(),

                TeacherOptions = teachers.Select(t => new SelectListItem
                {
                    Value = t.Id.ToString(),
                    Text = $"{t.FirstName} {t.LastName}",
                    Selected = t.Id == classEntity.TeacherId
                }).ToList(),
        
                AllCourseMetadata = courses.Select(c => new CourseMetadataDTO
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    CourseUnit = c.Units,
                }).ToList()
            };
            return dto;
        }

        public async Task<Class?> GetClassByIdIncludeInactiveAsync(int id)
        {
            return await _classManagementRepository.GetByIdIncludeInactiveAsync(id);
        }

        public async Task<OperationResultDTO> CreateClassAsync(ClassCreateCommandDTO model)
        {
            var result = new OperationResultDTO { Success = true };
            if (TimeSpan.TryParse(model.StartTime, out var startTime) && TimeSpan.TryParse(model.EndTime, out var endTime))
            {
                if (endTime <= startTime)
                {
                    result.Success = false;
                    result.Errors.Add("End time must be after start time.");
                }
            }
            else
            {
                result.Success = false;
                result.Errors.Add("Invalid time format.");
            }

            if (!result.Success) return result;

            var course = await _courseManagementRepository.GetByIdAsync(model.SelectedCourseId);
            if (course == null)
            {
                return new OperationResultDTO { Success = false, Errors = { "Invalid Course ID provided." } };
            }
            
            string finalSchedule = ReassembleSchedule(model.SelectedDays, model.StartTime, model.EndTime);
            var newClass = new Class
            {
                CourseId = model.SelectedCourseId,
                TeacherId = model.SelectedTeacherId,
                Capacity = model.Capacity,
                Schedule = finalSchedule,
                IsActive = false,
                
                YearLevel = course.YearLevel,
                Semester = course.AvailableSemester,
                
                JoinCode = "TEMP", 
                JoinCodeGeneratedAt = DateTime.UtcNow,
            };

            await _classManagementRepository.AddAsync(newClass);

            if (newClass.Id <= 0)
            {
                return new OperationResultDTO { Success = false, Message = "Failed to generate Class ID during save." };
            }

            string joinCodeBase = $"{course.CourseCode}-{newClass.Id}";
            newClass.JoinCode = Regex.Replace(joinCodeBase, "[^a-zA-Z0-9]", "");
            newClass.JoinCodeGeneratedAt = DateTime.UtcNow;

            await _classManagementRepository.UpdateAsync(newClass); 

            result.Message = $"Class {newClass.Id} created successfully.";
            return result;
        }

        public async Task<ClassCreateDTO> GetClassCreateModelAsync()
        {
            var nextId = await _classManagementRepository.GetNextClassIdAsync();

            var courses = await _courseManagementRepository.GetAllAsync();
            var allUsers = await _userManagementRepository.GetAllUsersAsync();
            var teachers = allUsers.Where(u => u.Role == "Teacher").ToList();

            var dto = new ClassCreateDTO
            {
                EDPCode = nextId,
                
                CourseOptions = courses.Select(c => new SelectListItem {
                    Value = c.Id.ToString(),
                    Text = c.CourseCode,
                }).ToList(),

                TeacherOptions = teachers.Select(t => new SelectListItem {
                    Value = t.Id.ToString(),
                    Text = $"{t.FirstName} {t.LastName}",
                }).ToList(),
                
                AllCourseMetadata = courses.Select(c => new CourseMetadataDTO
                {
                    Id = c.Id,
                    CourseCode = c.CourseCode,
                    CourseName = c.CourseName,
                    CourseUnit = c.Units,
                }).ToList()
            };

            return dto;
        }

        public async Task<OperationResultDTO> UpdateClassAsync(ClassEditCommandDTO model)
        {
            var result = new OperationResultDTO { Success = true };

            if (TimeSpan.TryParse(model.StartTime, out var startTime) && 
                TimeSpan.TryParse(model.EndTime, out var endTime))
            {
                if (endTime <= startTime)
                {
                    result.Success = false;
                    result.Errors.Add("End time must be after start time.");
                    return result;
                }
            }

            string newSchedule = ReassembleSchedule(model.SelectedDays, model.StartTime, model.EndTime);
            
            if (model.TeacherId <= 0)
            {
                result.Success = false;
                result.Errors.Add("A valid teacher must be selected.");
                return result;
            }

            var existingClass = await _classManagementRepository.GetByIdAsync(model.EDPCode);
            if (existingClass == null)
            {
                return new OperationResultDTO { Success = false, Message = "Class not found for update." };
            }

            existingClass.TeacherId = model.TeacherId;
            existingClass.Schedule = newSchedule;
            existingClass.Capacity = model.Capacity;
            existingClass.IsActive = model.Status;

            existingClass.JoinCode ??= "TEMP";

            try
            {
                await _classManagementRepository.UpdateAsync(existingClass);
                result.Message = $"Class {model.EDPCode} updated successfully.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "A system error occurred during the update.";
                result.Errors.Add(ex.Message);
            }
            
            return result;
        }

        public async Task<ClassDeleteDTO?> GetClassDeleteModelAsync(int id)
        {
            var classEntity = await _classManagementRepository.GetByIdAsync(id);
            
            if (classEntity == null)
                return null;

            var hasStudents = await _classManagementRepository.HasEnrolledStudentsAsync(id);

            var dto = new ClassDeleteDTO
            {
                EDPCode = classEntity.Id,
                CourseCode = classEntity.Course?.CourseCode ?? "N/A",
                CourseName = classEntity.Course?.CourseName ?? "N/A",
                IsActive = classEntity.IsActive,
                HasEnrolledStudents = hasStudents
            };

            return dto;
        }

        public async Task<OperationResultDTO> DeleteClassAsync(int id)
        {
            var result = new OperationResultDTO { Success = true };

            try
            {
                var classEntity = await _classManagementRepository.GetByIdAsync(id);
                
                if (classEntity == null)
                {
                    result.Success = false;
                    result.Message = "Class not found.";
                    return result;
                }

                if (classEntity.IsActive)
                {
                    result.Success = false;
                    result.Message = "Cannot delete an active class. Please deactivate it first.";
                    return result;
                }

                var hasStudents = await _classManagementRepository.HasEnrolledStudentsAsync(id);
                if (hasStudents)
                {
                    result.Success = false;
                    result.Message = "Cannot delete a class with enrolled students.";
                    return result;
                }

                await _classManagementRepository.DeleteAsync(classEntity.Id);
                
                result.Message = $"Class {id} deleted successfully.";
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = "An error occurred while deleting the class.";
                result.Errors.Add(ex.Message);
            }

            return result;
        }

        public async Task<bool> ActivateClassAsync(int classId, int teacherId)
        {
            var classEntity = await _classManagementRepository.GetByIdIncludeInactiveAsync(classId);
            if (classEntity == null)
                throw new Exception("Class not found with the provided EDP code.");

            // Check if class is already active and assigned to a different teacher
            if (classEntity.IsActive && classEntity.TeacherId.HasValue && classEntity.TeacherId.Value != teacherId)
            {
                throw new InvalidOperationException("This class is already active and assigned to another teacher.");
            }

            // Assign teacher and activate the class
            classEntity.TeacherId = teacherId;
            classEntity.IsActive = true;

            await _classManagementRepository.UpdateAsync(classEntity);
            return true;
        }

        public ScheduleDTO ParseSchedule(string schedule)
        {
            var days = new List<string>();
            string startTime = "";
            string endTime = "";

            if (!string.IsNullOrEmpty(schedule))
            {
                var parts = schedule.Split(',');
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

            return new ScheduleDTO
            {
                Days = days.ToArray(),
                StartTime = startTime,
                EndTime = endTime
            };
        }

        private string ReassembleSchedule(string[] days, string startTime, string endTime)
        {
            var abbreviations = days
                .Select(day => day switch
                {
                    "Monday" => "M",
                    "Tuesday" => "T",
                    "Wednesday" => "W",
                    "Thursday" => "TH",
                    "Friday" => "F",
                    "Saturday" => "S",
                    _ => ""
                })
                .Where(abbr => !string.IsNullOrEmpty(abbr))
                .ToArray();

            string dayString = string.Join("", abbreviations);

            string timeString = "";

            if (DateTime.TryParseExact(startTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var startDateTime) &&
                DateTime.TryParseExact(endTime, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var endDateTime))
            {
                string formattedStartTime = startDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
                string formattedEndTime = endDateTime.ToString("h:mm tt", CultureInfo.InvariantCulture);
                
                timeString = $"{formattedStartTime}–{formattedEndTime}";
            }
            
            if (string.IsNullOrEmpty(dayString) && string.IsNullOrEmpty(timeString))
            {
                return string.Empty;
            }
            
            return $"{dayString}, {timeString}";
        }
    }
}
