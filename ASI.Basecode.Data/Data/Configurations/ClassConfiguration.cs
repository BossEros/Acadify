using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Data.Configurations
{
    public class ClassConfiguration : IEntityTypeConfiguration<Class>
    {
        public void Configure(EntityTypeBuilder<Class> builder)
        {
            builder.ToTable("classes");

            builder.HasKey(c => c.Id);

            builder.HasIndex(c => c.JoinCode)
                .IsUnique();

            builder.Property(c => c.Id)
                .HasColumnName("id")
                .UseIdentityColumn();

            builder.Property(c => c.CourseId)
                .HasColumnName("course_id")
                .IsRequired();

            builder.Property(c => c.TeacherId)
                .HasColumnName("teacher_id");

            builder.Property(c => c.Semester)
                .HasColumnName("semester")
                .HasColumnType("SMALLINT")
                .IsRequired();

            builder.Property(c => c.YearLevel)
                .HasColumnName("year_level")
                .HasColumnType("SMALLINT")
                .IsRequired();

            builder.Property(c => c.Schedule)
                .HasColumnName("schedule")
                .HasColumnType("VARCHAR(100)")
                .IsRequired();

            builder.Property(c => c.Capacity)
                .HasColumnName("capacity")
                .HasColumnType("INTEGER")
                .IsRequired();

            builder.Property(c => c.JoinCode)
                .HasColumnName("join_code")
                .HasColumnType("VARCHAR(50)")
                .IsRequired();

            builder.Property(c => c.JoinCodeGeneratedAt)
                .HasColumnName("join_code_generated_at")
                .HasColumnType("TIMESTAMPTZ")
                .HasDefaultValueSql("NOW()");

            builder.Property(c => c.IsActive)
                .HasColumnName("is_active")
                .HasDefaultValue(true);

            builder.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("TIMESTAMPTZ")
                .HasDefaultValueSql("NOW()");

            // Relationship
            builder.HasOne(c => c.Course)
                .WithMany(co => co.Classes)
                .HasForeignKey(c => c.CourseId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(c => c.Teacher)
                .WithMany(u => u.ClassesTeaching)
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}