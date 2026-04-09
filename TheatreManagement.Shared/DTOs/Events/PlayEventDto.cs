using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Events
{
    public class PlayEventDto 
    {
        public int PlayEventId { get; set; }

        public int PlayId { get; set; }
        public int EventId { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public List<PlayEventRoleDisplay> RoleDisplays { get; set; } = new();

        public Dictionary<int, List<int>> SelectedEmployees { get; set; } = new();
        
        public int CastId { get; set; }
    }

    public class PlayEventRoleDisplay
    {
        public string RoleName { get; set; }
        public string EmployeeNames { get; set; }
        public int Count { get; set; }
    }

}
