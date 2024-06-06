using Microsoft.EntityFrameworkCore;
using QADraft.Models;

namespace QADraft.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<GeekQA> GeekQAs { get; set; }
        public DbSet<Events> Events { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<GeekQA>()
                .HasOne(e => e.CommittedBy)
                .WithMany()
                .HasForeignKey(e => e.CommittedById)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GeekQA>()
                .HasOne(e => e.FoundBy)
                .WithMany()
                .HasForeignKey(e => e.FoundById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
