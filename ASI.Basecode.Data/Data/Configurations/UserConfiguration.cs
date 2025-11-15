using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ASI.Basecode.Data.Models;

namespace ASI.Basecode.Data.Data.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.Property(u => u.Id)
                .HasColumnName("id");

            builder.Property(u => u.IdNumber)
                .HasColumnName("id_number")
                .HasColumnType("INTEGER");

            builder.Property(u => u.Email)
                .HasColumnName("email");

            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash");

            builder.Property(u => u.FirstName)
                .HasColumnName("first_name")
                .HasColumnType("VARCHAR(100)")
                .IsRequired();
            
            builder.Property(u => u.LastName)
                .HasColumnName("last_name")
                .HasColumnType("VARCHAR(100)")
                .IsRequired();

            builder.Property(u => u.ProfilePicture)
                .HasColumnName("profile_picture")
                .HasColumnType("VARCHAR(255)");

            builder.Property(u => u.IsApproved)
                .HasColumnName("is_approved");

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("TIMESTAMPTZ")
                .HasDefaultValueSql("NOW()");

            // Keep Identity columns mapped to avoid EF warnings; do not ignore them
            builder.Property(u => u.UserName).HasColumnName("user_name");
            builder.Property(u => u.NormalizedUserName).HasColumnName("normalized_user_name");
            builder.Property(u => u.NormalizedEmail).HasColumnName("normalized_email");
            builder.Property(u => u.SecurityStamp).HasColumnName("security_stamp");
            builder.Property(u => u.ConcurrencyStamp).HasColumnName("concurrency_stamp");
            builder.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");   

            // Ignore unused Identity columns to clean up the database schema
            builder.Ignore(u => u.PhoneNumber);
            builder.Ignore(u => u.PhoneNumberConfirmed);
            builder.Ignore(u => u.TwoFactorEnabled);
            builder.Ignore(u => u.LockoutEnd);
            builder.Ignore(u => u.LockoutEnabled);
            builder.Ignore(u => u.AccessFailedCount);
        }
     }
}
