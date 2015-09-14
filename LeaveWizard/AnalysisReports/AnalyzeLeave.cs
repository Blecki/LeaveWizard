using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard.AnalysisReports
{
    public class LeaveSummary : LeaveAnalysis
    {
        public LeaveSummary(LeaveChart Chart) : base(Chart)
        {
            this.ReportName = "Leave Summary";
        }

        public override LeaveAnalysisResults Analyze(LeaveChart Chart)
        {
            var usageTable = new Dictionary<String, Dictionary<String, int>>();

            foreach (var leaveEntry in Chart.EnumerateLeave(StartDate, EndDate).Where(l => !Constants.PropogatableLeaveTypes.Contains(l.LeaveType)))
            {
                Dictionary<String, int> carrierEntry = null;
                if (!usageTable.TryGetValue(leaveEntry.Carrier, out carrierEntry))
                {
                    carrierEntry = new Dictionary<string, int>();
                    usageTable.Add(leaveEntry.Carrier, carrierEntry);
                }

                if (carrierEntry.ContainsKey(leaveEntry.LeaveType))
                    carrierEntry[leaveEntry.LeaveType] += 1;
                else
                    carrierEntry.Add(leaveEntry.LeaveType, 1);
            }

            var result = new LeaveAnalysisResults();
            result.ColumnNames.Add("Carrier");

            foreach (var carrierEntry in usageTable)
            {
                var row = new LeaveAnalysisRow { Name = carrierEntry.Key };
                foreach (var leaveType in carrierEntry.Value)
                {
                    if (!result.ColumnNames.Contains(leaveType.Key))
                        result.ColumnNames.Add(leaveType.Key);
                    row.Columns.Add(leaveType.Key, leaveType.Value.ToString());
                }
                result.Rows.Add(row);
            }

            return result;
        }
    }
}
