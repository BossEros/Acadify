# Database Schema

## Overview

The Student Performance Tracker uses PostgreSQL as its primary database, hosted on Supabase. The schema is designed to support academic management with proper relationships and data integrity.

## Entity Relationship Diagram

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│    User     │────▶│   Class     │────▶│   Course    │
│             │ 1:N │             │ N:1 │             │
│ (Teacher)   │     │             │     │             │
└─────────────┘     └─────────────┘     └─────────────┘
                           │ 1:N
                           ▼
                    ┌─────────────┐     ┌─────────────┐
                    │ Enrollment  │────▶│    User     │
                    │             │ N:1 │             │
                    │             │     │ (Student)   │
                    └─────────────┘     └─────────────┘
                           │ 1:1
                           ▼
                    ┌─────────────┐
                    │    Grade    │
                    │             │
                    │             │
                    └─────────────┘
```

## Core Entities

### User (extends IdentityUser<int>)

The User entity extends ASP.NET Core Identity's `IdentityUser` to include additional properties for the academic system.

**Properties:**
```csharp
public class User : IdentityUser<int>
{
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? ProfilePicture { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Class> ClassesTeaching { get; set; } = new List<Class>();
    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
```

**Key Features:**
- **Primary Key:** `Id` (int) - inherited from IdentityUser
- **Built-in Properties:** Email, UserName, PasswordHash, etc. from Identity
- **Custom Properties:** FirstName, LastName, ProfilePicture, IsApproved
- **Audit Trail:** CreatedAt timestamp
- **Relationships:** Can teach classes (as Teacher) or enroll in classes (as Student)

### Course

Represents academic courses offered by the institution.

**Properties:**
```csharp
public class Course
{
    public int Id { get; set; }
    public string CourseCode { get; set; } = null!;
    public string CourseName { get; set; } = null!;
    public string? Description { get; set; }
    public short Units { get; set; }
    public short YearLevel { get; set; }
    public short AvailableSemester { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
}
```

**Business Rules:**
- **CourseCode** must be unique (e.g., "CS101", "MATH201")
- **Units** typically range from 1-6 credit hours
- **YearLevel** indicates target student year (1-4)
- **AvailableSemester** indicates when course is offered (1=Fall, 2=Spring, 3=Both)

### Class

Represents a specific instance of a course being taught in a particular semester.

**Properties:**
```csharp
public class Class
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public int TeacherId { get; set; }
    public short Semester { get; set; }
    public short YearLevel { get; set; }
    public string Schedule { get; set; } = null!;
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
```

**Business Rules:**
- **JoinCode** must be unique and is used for student enrollment
- **Schedule** format: "MWF 10:00-11:00" or similar
- **Room** indicates physical or virtual classroom location
- **Semester** (1=Fall, 2=Spring, 3=Summer)

### Enrollment

Links students to specific classes they are enrolled in.

**Properties:**
```csharp
public class Enrollment
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public int ClassId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual User Student { get; set; } = null!;
    public virtual Class Class { get; set; } = null!;
    public virtual Grade? Grade { get; set; }
}
```

**Business Rules:**
- **Unique Constraint:** One student can only enroll once per class
- **Cascade Delete:** If class is deleted, enrollments are removed
- **Audit Trail:** EnrolledAt tracks when student joined

### Grade

Stores academic grades for student enrollments.

**Properties:**
```csharp
public class Grade
{
    public int Id { get; set; }
    public int EnrollmentId { get; set; }
    public decimal? MidtermGrade { get; set; }
    public decimal? FinalGrade { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation property
    public virtual Enrollment Enrollment { get; set; } = null!;
}
```

**Business Rules:**
- **Grade Scale:** Typically 0.0 - 4.0 or 0-100 depending on institution
- **Nullable Grades:** Allows for partial grading (midterm only, etc.)
- **Audit Trail:** CreatedAt and UpdatedAt for change tracking
- **Remarks:** Additional context for grades (e.g., "Excellent work", "Needs improvement")

## Relationships

### One-to-Many Relationships

#### User (Teacher) → Classes
```csharp
// One teacher can teach multiple classes
public virtual ICollection<Class> ClassesTeaching { get; set; }

// Each class has one teacher
public virtual User Teacher { get; set; } = null!;
```

#### Course → Classes
```csharp
// One course can have multiple class instances
public virtual ICollection<Class> Classes { get; set; }

// Each class belongs to one course
public virtual Course Course { get; set; } = null!;
```

#### Class → Enrollments
```csharp
// One class can have multiple student enrollments
public virtual ICollection<Enrollment> Enrollments { get; set; }

// Each enrollment belongs to one class
public virtual Class Class { get; set; } = null!;
```

#### User (Student) → Enrollments
```csharp
// One student can have multiple enrollments
public virtual ICollection<Enrollment> Enrollments { get; set; }

// Each enrollment belongs to one student
public virtual User Student { get; set; } = null!;
```

### One-to-One Relationships

#### Enrollment → Grade
```csharp
// Each enrollment can have one grade record
public virtual Grade? Grade { get; set; }

// Each grade belongs to one enrollment
public virtual Enrollment Enrollment { get; set; } = null!;
```

## Database Constraints

### Primary Keys
- All entities use `int` identity columns as primary keys
- Auto-incrementing values managed by PostgreSQL

### Foreign Keys
- **Class.CourseId** → Course.Id
- **Class.TeacherId** → User.Id
- **Enrollment.StudentId** → User.Id
- **Enrollment.ClassId** → Class.Id
- **Grade.EnrollmentId** → Enrollment.Id

### Unique Constraints
- **Course.CourseCode** - Prevents duplicate course codes
- **Class.JoinCode** - Ensures unique join codes
- **Enrollment (StudentId, ClassId)** - Prevents duplicate enrollments

### Check Constraints
- **Course.Units** - Must be between 1 and 6
- **Course.YearLevel** - Must be between 1 and 4
- **Grade values** - Must be within valid range (0.0-4.0 or 0-100)

## Indexes

### Performance Indexes
```sql
-- Frequently queried fields
CREATE INDEX IX_User_Email ON Users(Email);
CREATE INDEX IX_Class_JoinCode ON Classes(JoinCode);
CREATE INDEX IX_Enrollment_StudentId ON Enrollments(StudentId);
CREATE INDEX IX_Enrollment_ClassId ON Enrollments(ClassId);
```

### Composite Indexes
```sql
-- For complex queries
CREATE INDEX IX_Class_Semester_YearLevel ON Classes(Semester, YearLevel);
CREATE INDEX IX_Course_YearLevel_Semester ON Courses(YearLevel, AvailableSemester);
```

## Sample Data Relationships

### Example Scenario
```
Course: "CS101 - Introduction to Programming" (3 units, Year 1)
  └── Class: "CS101-A Fall 2024" (Teacher: John Doe, Room: "Lab 101")
      ├── Enrollment: Student "Jane Smith" (Enrolled: 2024-08-15)
      │   └── Grade: Midterm: 85.5, Final: 92.0, Remarks: "Excellent progress"
      └── Enrollment: Student "Bob Johnson" (Enrolled: 2024-08-16)
          └── Grade: Midterm: 78.0, Final: null, Remarks: "Needs to submit final project"
```

## Migration History

The database schema is managed through Entity Framework Core migrations:

- **Initial Migration** - Creates base tables and relationships
- **Identity Integration** - Adds ASP.NET Core Identity tables
- **Audit Fields** - Adds CreatedAt, UpdatedAt timestamps
- **Grade Enhancements** - Adds Remarks field and decimal precision

---

*This schema provides a solid foundation for academic management while maintaining data integrity and supporting future enhancements.*