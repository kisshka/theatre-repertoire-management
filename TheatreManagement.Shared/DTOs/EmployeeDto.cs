using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class EmployeeDto
    {
        public int EmployeeId { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? FatherName { get; set; }
        public string? Post { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }
        // ссылки 
        public string? UserId { get; set; }
        public UserDto? User { get; set; }
        //public List<EmployeeRoleDto> EmployeeRoles { get; set; } = new();
    }
}
