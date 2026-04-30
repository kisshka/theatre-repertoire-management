using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.ConflictChecker
{
    public class EventConflictPreview
    {
        public int EventId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Type { get; set; }

        public string? Hall { get; set; }
        public List<int> EmployeeIds { get; set; } = new();
    }
}
