using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.WebApp.ViewModels.ClassManagement
{
    public class EditClassViewModel : IValidatableObject
    {
        [Required(ErrorMessage = "Course is required")]
        [Display(Name = "Course")]
        public int? CourseId { get; set; }

        [Display(Name = "Teacher")]
        public int? TeacherId { get; set; }

        [Required(ErrorMessage = "Semester is required")]
        [Display(Name = "Semester")]
        public short Semester { get; set; }

        [Required(ErrorMessage = "Year Level is required")]
        [Display(Name = "Year Level")]
        public short YearLevel { get; set; }

        [Required(ErrorMessage = "Capacity is required")]
        [Display(Name = "Capacity")]
        [Range(1, int.MaxValue, ErrorMessage = "Capacity must be at least 1")]
        public int Capacity { get; set; }

        [Required(ErrorMessage = "At least one day must be selected")]
        [MinLength(1, ErrorMessage = "At least one day must be selected")]
        public string[] Days { get; set; } = Array.Empty<string>();

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        public string StartTime { get; set; } = string.Empty;

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        public string EndTime { get; set; } = string.Empty;

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
}
