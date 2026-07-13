using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Employees
{
    public class EmployeePlays
    {
        public int PlayId { get; set; }
        public string PlayName { get; set; }
        public List<EmployeeCasts> Casts { get; set; } = new();
    }
    public class EmployeeCasts
    {
        public int? CastId { get; set; }
        public string CastName { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}


