using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using TheatreManagement.Domain.Entities;
using System.Runtime.CompilerServices;

namespace TheatreManagement.Domain.Data
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }
        public virtual DbSet<Cast> Castes { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<PlayEvent> PlayEvents { get; set; }
        public virtual DbSet<Institution> Institutions { get; set; }
        public virtual DbSet<Play> Plays { get; set; }
        public virtual DbSet<RoleInPlay> RoleInPlays { get; set; }
        public virtual DbSet<Stationar> Stationars { get; set; }
        public virtual DbSet<Tour> Tours { get; set; }
        public override DbSet<User> Users { get; set; }

        [DbFunction("CustomLike", IsBuiltIn = false)]
        [MethodImpl(MethodImplOptions.NoOptimization | MethodImplOptions.NoInlining)]
        public static bool CustomLike(string text, string pattern)
        {
            if (text == null || pattern == null) return false;
            return text.Contains(pattern, StringComparison.OrdinalIgnoreCase);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Cast>().HasQueryFilter(i => i.DeletionTime == null);
            builder.Entity<Employee>().HasQueryFilter(i => i.DeletionTime == null);
            builder.Entity<Play>().HasQueryFilter(i => i.DeletionTime == null);
            builder.Entity<Event>().HasQueryFilter(i => i.DeletionTime == null);
            builder.Entity<User>().HasQueryFilter(i => i.DeletionTime == null);

            builder.Entity<Event>()
              .HasOne(e => e.Stationar)
              .WithOne(s => s.Event)
              .HasForeignKey<Stationar>(s => s.StationarId)
              .IsRequired(false);

            builder.Entity<Event>()
                .HasOne(e => e.Tour)
                .WithOne(t => t.Event)
                .HasForeignKey<Tour>(t => t.TourId)
                .IsRequired(false);
        }
    }
}