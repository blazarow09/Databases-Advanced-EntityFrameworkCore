using System;
using Microsoft.EntityFrameworkCore;
using P01_StudentSystem.Data.Models;

namespace P01_StudentSystem.Data
{
    public class StudentSystemContext : DbContext
    {
        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Resource> Resources { get; set; }
        public DbSet<Homework> HomeworkSubmissions { get; set; }
        public DbSet<StudentCourse> StudentCourses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(Config.ConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureStudentEntity(modelBuilder);

            ConfigureCourseEntity(modelBuilder);

            ConfigureResourceEntity(modelBuilder);

            ConfigureHomeworkEntity(modelBuilder);

            ConfigureStudentCourseEntity(modelBuilder);
        }

        private void ConfigureStudentCourseEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<StudentCourse>()
                .HasKey(t => new {t.StudentId, t.CourseId});
        }

        private void ConfigureHomeworkEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Homework>()
                .HasKey(h => h.HomeworkId);

            modelBuilder
                .Entity<Homework>()
                .Property(c => c.Content)
                .IsUnicode(false);

            modelBuilder
                .Entity<Homework>()
                .HasOne(e => e.Student)
                .WithMany(c => c.Homeworks)
                .HasForeignKey(e => e.StudentId);
        }

        private void ConfigureResourceEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Resource>()
                .HasKey(r => r.ResourceId);

            modelBuilder
            .Entity<Resource>()
            .Property(n => n.Name)
            .HasMaxLength(50)
            .IsUnicode(true);
        }

        private void ConfigureCourseEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Course>()
                .HasKey(c => c.CourseId);

            modelBuilder
                .Entity<Course>()
                .HasMany(c => c.StudentCourses)
                .WithOne(c => c.Course);

            modelBuilder
              .Entity<Course>()
              .Property(n => n.Name)
              .HasMaxLength(80)
              .IsUnicode(true);

            modelBuilder
              .Entity<Course>()
              .Property(d => d.Description)
              .IsUnicode(true);

            modelBuilder
              .Entity<Course>()
              .Property(p => p.Price)
              .HasColumnType("MONEY");
        }

        private void ConfigureStudentEntity(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Student>()
                .HasKey(p => p.StudentId);

            modelBuilder
                .Entity<Student>()
                .HasMany(x => x.StudentCourses)
                .WithOne(x => x.Student);

            modelBuilder
                .Entity<Student>()
                .Property(n => n.Name)
                .HasMaxLength(100)
                .IsUnicode(true)
                .IsRequired(true);

            modelBuilder
               .Entity<Student>()
               .Property(p => p.PhoneNumber)
               .HasMaxLength(10)
               .IsFixedLength(true)
               .IsUnicode(false)
               .IsRequired(false);
        }
    }
}