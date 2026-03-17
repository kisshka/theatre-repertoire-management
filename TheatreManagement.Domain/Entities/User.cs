using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Entities;

namespace Domain.Entities
{
    public class  User : IdentityUser
    {
        public string? Surname { get; set; }
        public string? Name { get; set; }
        public string? FatherName { get; set; }
        public DateTime? DeletionTime { get; set; }

        // ссылки
        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
        public List<Cast> Casts { get; set; } = new();
        public List<Play> Plays { get; set; } = new();
        public List<Event> Events { get; set; } = new();
        public List<RoleInPlay> RoleInPlays { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
    }
}