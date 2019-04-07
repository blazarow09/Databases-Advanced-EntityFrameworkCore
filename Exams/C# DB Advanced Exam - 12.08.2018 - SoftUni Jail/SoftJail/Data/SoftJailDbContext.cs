namespace SoftJail.Data
{
    using Microsoft.EntityFrameworkCore;
    using SoftJail.Data.Models;

    public class SoftJailDbContext : DbContext
    {
        public SoftJailDbContext()
        {
        }

        public SoftJailDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseSqlServer(Configuration.ConnectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Prisoner>()
                .HasOne(p => p.Cell)
                .WithMany(c => c.Prisoners)
                .HasForeignKey(p => p.CellId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Mail>()
                .HasOne(m => m.Prisoner)
                .WithMany(p => p.Mails)
                .HasForeignKey(m => m.PrisonerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Officer>()
                .HasOne(o => o.Department)
                .WithMany(d => d.Officers)
                .HasForeignKey(o => o.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Prisoner>()
               .HasOne(p => p.Cell)
               .WithMany(c => c.Prisoners)
               .HasForeignKey(p => p.CellId)
               .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OfficerPrisoner>()
                .ToTable("OfficersPrisoners");
                

            builder.Entity<OfficerPrisoner>()
                    .HasKey(x => new { x.OfficerId, x.PrisonerId });

            builder.Entity<OfficerPrisoner>()
                .HasOne(x => x.Prisoner)
                .WithMany(x => x.PrisonerOfficers)
                .HasForeignKey(op => op.PrisonerId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OfficerPrisoner>()
                .HasOne(x => x.Officer)
                .WithMany(x => x.OfficerPrisoners)
                .HasForeignKey(op => op.OfficerId)
                .OnDelete(DeleteBehavior.Restrict);
        }

        public DbSet<Cell> Cells { get; set; }

        public DbSet<Department> Departments { get; set; }

        public DbSet<Mail> Mails { get; set; }

        public DbSet<Officer> Officers { get; set; }

        public DbSet<Prisoner> Prisoners { get; set; }

        public DbSet<OfficerPrisoner> OfficersPrisoners { get; set; }
    }
}