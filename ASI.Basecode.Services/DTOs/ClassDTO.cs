using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using ASI.Basecode.Data.Models;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Services.DTOs
{
    public class ClassIndexDTO
    {
        public int EDPCode { get; set; }
        public required string? CourseCode { get; set; }
        public required string Description { get; set; }
        public int Units {get; set; }
        public required string? Schedule {get; set; }
        public required string? TeacherName { get; set; }
        public bool Status { get; set; }
        public int Capacity { get; set; }
        public short Semester { get; set; }
        public short YearLevel { get; set; }
        
        // Temporary Variables
        public int? TeacherId { get; set; }
        public ICollection<Enrollment>? Enrollments { get; set; }
    }

    public class ClassDetailsDTO
    {
        public int EDPCode { get; set; }
        public required string CourseCode { get; set; }
        public required string Description { get; set; }
        public int Units {get; set; }
        public short Semester { get; set; }
        public short YearLevel { get; set; }
        public required string Schedule {get; set; }
        public bool Status { get; set; }
        public int Capacity { get; set; }
        public DateTime DateCreated { get; set; }
        public required string? TeacherName { get; set; }
        public ICollection<EnrolledStudentDTO>? Enrollments { get; set; }
    }

    public class ClassEditCommandDTO
    {
        public int EDPCode { get; set; }

        [Required(ErrorMessage = "Teacher is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "A valid teacher must be selected.")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 60, ErrorMessage = "Capacity must be between 1 and 60.")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "Days are required.")]
        public required string[] SelectedDays { get; set; }

        [Required(ErrorMessage = "Start Time is required.")]
        public required string StartTime { get; set; }

        [Required(ErrorMessage = "End Time is required.")]
        public required string EndTime { get; set; }

        public bool Status { get; set; }
    }

    public class EnrolledStudentDTO
    {
        public int StudentId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public decimal? MidtermGrade { get; set; }
        public decimal? FinalGrade { get; set; }
        public string? Remarks { get; set; }
    }

    public class ClassCreateDTO
    {
        public int EDPCode { get; set; }
        public List<SelectListItem> CourseOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> TeacherOptions { get; set; } = new List<SelectListItem>();
        public List<CourseMetadataDTO> AllCourseMetadata { get; set; } = new List<CourseMetadataDTO>();
        
        public int SelectedCourseId { get; set; }
        public int SelectedTeacherId { get; set; }
    }

    public class ClassEditDTO
    {
        public int EDPCode { get; set; }
        public required string CourseCode { get; set; }
        public int TeacherId { get; set; }
        public required string TeacherName { get; set; }
        public short Semester { get; set; }
        public short YearLevel { get; set; }
        public bool Status { get; set; }
        public int Capacity { get; set; }

        public required string[] SelectedDays { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
        
        public required List<SelectListItem> CourseOptions { get; set; }
        public required List<SelectListItem> TeacherOptions { get; set; }
        
        public required List<CourseMetadataDTO> AllCourseMetadata { get; set; }
    }

    public class ClassDeleteDTO
    {
        public int EDPCode { get; set; }
        public required string CourseCode { get; set; }
        public required string CourseName { get; set; }
        public bool Status { get; set; }
        public bool HasEnrolledStudents { get; set; }
    }

    public class ClassCreateCommandDTO : IValidatableObject
    {
        [Required(ErrorMessage = "Course is required.")]
        public int SelectedCourseId { get; set; }

        [Required(ErrorMessage = "Teacher is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "A valid teacher must be selected.")] // Replaces your 'viewModel.TeacherId == 0' check
        public int SelectedTeacherId { get; set; }

        [Required(ErrorMessage = "Capacity is required.")]
        [Range(1, 60, ErrorMessage = "Capacity must be between 1 and 60.")]
        public short? Capacity { get; set; }

        [Required(ErrorMessage = "At least one day must be selected")]
        [MinLength(1, ErrorMessage = "At least one day must be selected")]
        public required string[] SelectedDays { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public required string StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public required string EndTime { get; set; }

        // Time Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!TimeSpan.TryParse(StartTime, out var start))
            {
                yield return new ValidationResult(
                    "Invalid start time format.",
                    new[] { nameof(StartTime) });
                yield break;
            }

            if (!TimeSpan.TryParse(EndTime, out var end))
            {
                yield return new ValidationResult(
                    "Invalid end time format.",
                    new[] { nameof(EndTime) });
                yield break;
            }

            TimeSpan earliest = new TimeSpan(6, 0, 0);
            TimeSpan latest = new TimeSpan(23, 0, 0);

            if (start < earliest || start > latest)
                yield return new ValidationResult(
                    "Start time must be between 6:00 AM and 10:00 PM.",
                    new[] { nameof(StartTime) });

            if (end < earliest || end > latest)
                yield return new ValidationResult(
                    "End time must be between 7:00 AM and 11:00 PM.",
                    new[] { nameof(EndTime) });

            var duration = end - start;

            if (duration < TimeSpan.FromMinutes(60))
                yield return new ValidationResult(
                    "Class must be at least 60 minutes long.",
                    new[] { nameof(EndTime) });

            if (duration > TimeSpan.FromHours(6))
                yield return new ValidationResult(
                    "Class cannot exceed 6 hours.",
                    new[] { nameof(EndTime) });

            if (end <= start)
                yield return new ValidationResult(
                    "End time must be later than start time.",
                    new[] { nameof(EndTime) });
        }
    }

    public class ScheduleDTO
    {
        public required string[] Days { get; set; }
        public required string StartTime { get; set; }
        public required string EndTime { get; set; }
    }

    public class CourseMetadataDTO 
    {
        public int Id { get; set; }
        public required string CourseCode { get; set; }
        public required string CourseName { get; set; }
        public int CourseUnit { get; set; }
        public short YearLevel { get; set; }
        public short AvailableSemester { get; set; }
    }

    public class OperationResultDTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<string> Errors { get; set; } = new List<string>();

        public static OperationResultDTO SuccessResult(string message)
        {
            return new OperationResultDTO { Success = true, Message = message };
        }

        public static OperationResultDTO FailureResult(List<string> errors, string message = "Operation failed due to validation errors.")
        {
            return new OperationResultDTO { Success = false, Message = message, Errors = errors };
        }
    }
}
