using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Domain.Entities
{
    public class PlayEvent
    {
        public int PlayEventId {  get; set; }
        
        public int PlayId { get; set; }
        public int EventId { get; set; }

        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
         
        public Play Play { get; set; }
        public Event Event { get; set; }

    }
}
