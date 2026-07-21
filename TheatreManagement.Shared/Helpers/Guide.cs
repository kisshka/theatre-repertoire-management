using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.Helpers
{
    public class Guide
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsUsed { get; set; }
    }
}
