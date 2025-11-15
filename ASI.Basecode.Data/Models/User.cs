using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace ASI.Basecode.Data.Models
{
    public class User : IdentityUser<int>
    {
        public int IdNumber { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? ProfilePicture { get; set; }
        public bool IsApproved { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Class> ClassesTeaching { get; set; } = new List<Class>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
       
    }
}   