using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public partial class LeaveChart
    {
        public List<WeekData> Weeks = new List<WeekData>();

        public IEnumerable<String> EnumerateAllCarriers(DateTime StartDate, DateTime EndDate)
        {
            var result = new List<String>();
            foreach (var week in EnumerateWeeks(StartDate, EndDate))
            {
                result.AddRange(week.Substitutes.Select(s => s.Name));
                result.AddRange(week.Regulars.Select(r => r.Name));
            }

            foreach (var carrier in result.Distinct().OrderBy(s => s))
                yield return carrier;
        }

        public IEnumerable<String> EnumerateAllCarriers()
        {
            var result = new List<String>();
            foreach (var week in Weeks)
            {
                result.AddRange(week.Substitutes.Select(s => s.Name));
                result.AddRange(week.Regulars.Select(r => r.Name));
            }

            foreach (var carrier in result.Distinct().OrderBy(s => s))
                yield return carrier;
        }
    }
}
