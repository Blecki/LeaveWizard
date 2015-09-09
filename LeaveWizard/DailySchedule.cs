using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public class DailySchedule
    {
        public bool IsHoliday;
        public List<LeaveEntry> ReliefDays = new List<LeaveEntry>();
        public List<LeaveEntry> DeniedLeave = new List<LeaveEntry>();
        public static String PropogatableLeaveTypes = "KJPVW";

        public DailySchedule CloneForNewWeek()
        {
            return new DailySchedule
            {
                IsHoliday = false, //Holiday's are never propogated forward.
                ReliefDays = new List<LeaveEntry>(this.ReliefDays.Where(rd => PropogatableLeaveTypes.Contains(rd.LeaveType)).Select(rd => rd.CloneForNewWeek()))
            };
        }
    }
}
