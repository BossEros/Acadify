using System.ComponentModel.DataAnnotations;

namespace ASI.Basecode.Data.Models
{
    public class Class
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Course is required.")]
        public int CourseId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = null!;
        
        [Required(ErrorMessage = "Units is required.")]
        public int Units { get; set; }

        [Required(ErrorMessage = "Teacher is required.")]
        public int TeacherId { get; set; }

        [Required(ErrorMessage = "Semester is required.")]
        public short Semester { get; set; }

        [Required(ErrorMessage = "Year level is required.")]
        public short YearLevel { get; set; }

        [Required(ErrorMessage = "Schedule is required.")]
        public string Schedule { get; set; } = null!;

        [Required(ErrorMessage = "Room is required.")]
        public string Room { get; set; } = null!;

        public string JoinCode { get; set; } = null!;

        public DateTime JoinCodeGeneratedAt { get; set; } = DateTime.UtcNow;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Course Course { get; set; } = null!;
        public virtual User Teacher { get; set; } = null!;
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
