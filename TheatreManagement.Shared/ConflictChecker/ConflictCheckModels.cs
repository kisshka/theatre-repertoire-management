using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.ConflictChecker
{
    public class ConflictCheckRequest
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Type { get; set; }
        public string? Hall { get; set; }
        public List<int> EmployeeIds { get; set; } = new();
        public int? ExcludeEventId { get; set; }
    }

    public class ConflictCheckResponse
    {
        public bool HasConflicts { get; set; }
        public List<string> Warnings { get; set; } = new();
    }
}
