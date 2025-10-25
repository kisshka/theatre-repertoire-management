using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
namespace Domain.Entities
{
    public class DataContext : IdentityDbContext<User>
    {
        public DataContext(DbContextOptions<DataContext> options)
        : base(options) { }
        public virtual DbSet<Cast> Castes { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<EmployeeRole> EmployeeRoles { get; set; }
        public virtual DbSet<Event> Events { get; set; }
        public virtual DbSet<Institution> Institutions { get; set; }
        public virtual DbSet<Play> Plays { get; set; }
        public virtual DbSet<RoleInPlay> RoleInPlays { get; set; }
        public virtual DbSet<Stationar> Stationars { get; set; }
        public virtual DbSet<Tour> Tours { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}