using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard.AnalysisReports
{
    public class LeaveSummary : LeaveAnalysis
    {
        public override string ReportName
        {
            get
            {
                return "Leave Summary";
            }
        }

        public override LeaveAnalysisResults Analyze(LeaveChart Chart)
        {
            var result = new LeaveAnalysisResults();
            result.ColumnNames.Add("Carrier");

            for (var weekIndex = 0; weekIndex < Chart.Weeks.Count; ++weekIndex)
            {
                var previousWeek = weekIndex == 0 ? null : Chart.Weeks[weekIndex - 1];
                Chart.Weeks[weekIndex].Propogate(previousWeek);

                for (var dayIndex = 0; dayIndex < 7; ++dayIndex)
                    foreach (var leaveEntry in Chart.Weeks[weekIndex].DailySchedules[dayIndex].ReliefDays)
                        if (!Constants.PropogatableLeaveTypes.Contains(leaveEntry.LeaveType))
                        {
                            var carrierEntry = result.Rows.Find(c => c.Name == leaveEntry.Carrier);
                            if (carrierEntry == null)
                            {
                                carrierEntry = new LeaveAnalysisRow { Name = leaveEntry.Carrier };
                                result.Rows.Add(carrierEntry);
                            }

                            if (!result.ColumnNames.Contains(leaveEntry.LeaveType))
                                result.ColumnNames.Add(leaveEntry.LeaveType);

                            if (carrierEntry.Columns.ContainsKey(leaveEntry.LeaveType))
                                carrierEntry.Columns[leaveEntry.LeaveType] += 1;
                            else
                                carrierEntry.Columns.Add(leaveEntry.LeaveType, 1);
                        }
            }

            return result;
        }
    }
}
