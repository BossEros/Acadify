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
        public string? Schedule { get; set; }
        public int Capacity { get; set; }
        public string? JoinCode { get; set; }
        public DateTime JoinCodeGeneratedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool Status 
        { 
            get 
            {
                if (!IsActive) return false;
                
                var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
                if (CreatedAt < sixMonthsAgo) return false;
                
                return true;
            }
            set
            {
                IsActive = value;
            }
        }

        // Navigation properties
        public virtual Course? Course { get; set; }
        public virtual User? Teacher { get; set; }
        public virtual ICollection<Enrollment>? Enrollments { get; set; } = new List<Enrollment>();
    }
}