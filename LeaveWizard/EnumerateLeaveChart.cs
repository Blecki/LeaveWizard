using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public partial class LeaveChart
    {
        public IEnumerable<WeekData> EnumerateWeeks(DateTime StartDate, DateTime EndDate)
        {
            var currentWeek = 0;

            while (true)
            {
                if (currentWeek >= Weeks.Count) break;
                var previousWeek = (currentWeek == 0 ? null : Weeks[currentWeek - 1]);
                Weeks[currentWeek].Propogate(previousWeek);
                if (Weeks[currentWeek].SaturdayDate > EndDate) break;
                if (Weeks[currentWeek].SaturdayDate + TimeSpan.FromDays(7) > StartDate) yield return Weeks[currentWeek];
                currentWeek += 1;
            }
        }

        public IEnumerable<DailySchedule> EnumerateDays(DateTime StartDate, DateTime EndDate)
        {
            foreach (var week in EnumerateWeeks(StartDate, EndDate))
            {
                for (var day = 0; day < 7; ++day)
                    if (week.SaturdayDate + TimeSpan.FromDays(day) >= StartDate &&
                        week.SaturdayDate + TimeSpan.FromDays(day) <= EndDate)
                        yield return week.DailySchedules[day];
            }
        }

        public IEnumerable<LeaveEntry> EnumerateLeave(DateTime StartDate, DateTime EndDate)
        {
            foreach (var day in EnumerateDays(StartDate, EndDate))
                foreach (var entry in day.ReliefDays)
                    yield return entry;
        }

        public class PairedLeaveEntry
        {
            public DateTime Date;
            public LeaveEntry LeaveEntry;
        }

        public IEnumerable<PairedLeaveEntry> EnumerateLeaveWithDates(DateTime StartDate, DateTime EndDate)
        {
            foreach (var week in EnumerateWeeks(StartDate, EndDate))
            {
                for (var day = 0; day < 7; ++day)
                    if (week.SaturdayDate + TimeSpan.FromDays(day) >= StartDate &&
                        week.SaturdayDate + TimeSpan.FromDays(day) <= EndDate)
                        foreach (var leaveEntry in week.DailySchedules[day].ReliefDays)
                            yield return new PairedLeaveEntry { Date = week.SaturdayDate + TimeSpan.FromDays(day), LeaveEntry = leaveEntry };
            }
        }
    }

}
