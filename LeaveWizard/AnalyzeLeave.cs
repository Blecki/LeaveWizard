using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public static class AnalyzeLeave
    {
        public static LeaveAnalysisResults BasicAnalysis(LeaveChart Chart)
        {
            var result = new LeaveAnalysisResults();

            for (var weekIndex = 0; weekIndex < Chart.Weeks.Count; ++weekIndex)
            {
                var previousWeek = weekIndex == 0 ? null : Chart.Weeks[weekIndex - 1];
                Chart.Weeks[weekIndex].Propogate(previousWeek);

                for (var dayIndex = 0; dayIndex < 7; ++dayIndex)
                    foreach (var leaveEntry in Chart.Weeks[weekIndex].DailySchedules[dayIndex].ReliefDays)
                        if (!Constants.PropogatableLeaveTypes.Contains(leaveEntry.LeaveType))
                        {
                            var carrierEntry = result.Find(c => c.Carrier == leaveEntry.Carrier);
                            if (carrierEntry == null)
                            {
                                carrierEntry = new LeaveAnalysisEntry { Carrier = leaveEntry.Carrier };
                                result.Add(carrierEntry);
                            }

                            if (carrierEntry.LeaveUsage.ContainsKey(leaveEntry.LeaveType))
                                carrierEntry.LeaveUsage[leaveEntry.LeaveType] += 1;
                            else
                                carrierEntry.LeaveUsage.Add(leaveEntry.LeaveType, 1);
                        }
            }

            return result;
        }
    }
}
