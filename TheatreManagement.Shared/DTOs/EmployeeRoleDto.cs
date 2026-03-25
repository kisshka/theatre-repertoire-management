using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class EmployeeRoleDto
    {
        public int EmployeeRoleId { get; set; }
        public DateTime LastEditTime { get; set; }

        // ссылки
        //public int EmployeeId { get; set; }
        //public Employee? Employee { get; set; }
        public int RoleInPlayId { get; set; }
        public RoleDto? RoleInPlay { get; set; }
        //public int EventId { get; set; }
        //public Event? Event { get; set; }
        public int CastId { get; set; }
        public CastDto? Cast { get; set; }
        public string? UserId { get; set; }
        public UserDto? User { get; set; }
    }
}
