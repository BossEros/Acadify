using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public class Class
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public int? TeacherId { get; set; }
        public short Semester { get; set; }
        public short YearLevel { get; set; }
        // public short Units { get; set; }
        public string? Schedule { get; set; }
        public string? JoinCode { get; set; }
        public DateTime JoinCodeGeneratedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Course? Course { get; set; }
        public virtual User? Teacher { get; set; }
        public virtual ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();
    }
}