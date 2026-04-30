using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.ConflictChecker
{
    public class ScheduleConflictChecker
    {
        public static bool IsConflicting(DateTime start1, DateTime end1, DateTime start2, DateTime end2)
        {
            return start1 < end2 && start2 < end1;
        }

    }
}
