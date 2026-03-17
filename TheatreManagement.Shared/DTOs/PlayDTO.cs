using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class PlayDTO
    {
            public int PlayId { get; set; }
            public string? Name { get; set; }
            public string? Duration { get; set; }
            public string? Status { get; set; }
            public int AgeCategory { get; set; }
            public DateTime LastEditTime { get; set; }
            public bool IsDeleted { get; set; }
    }
}
