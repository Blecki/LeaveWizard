using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard.AnalysisReports
{
    public class SubstituteWorkingDays : LeaveAnalysis
    {
        public SubstituteWorkingDays(LeaveChart Chart) : base(Chart)
        {
            this.ReportName = "Substitute Working Days";
            this.Summary = "Count how many days a substitute has worked or is scheduled to work within the time period.";
        }
        
        public override LeaveAnalysisResults Analyze(LeaveChart Data)
        {
            var dayCount = new Dictionary<String, int>();

            foreach (var leaveEntry in Data.EnumerateLeave(StartDate, EndDate))
                if (dayCount.ContainsKey(leaveEntry.Substitute)) dayCount[leaveEntry.Substitute] += 1;
                else dayCount.Add(leaveEntry.Substitute, 1);

            var result = new LeaveAnalysisResults();
            result.ColumnNames.Add("Carrier");
            result.ColumnNames.Add("Days Worked");

            foreach (var sub in dayCount)
            {
                result.Rows.Add(new LeaveAnalysisRow { Name = sub.Key });
                result.Rows[result.Rows.Count - 1].Columns.Add("Days Worked", sub.Value.ToString());
            }

            result.SortRowsAlphabetically();
            return result;
        }
    }
}
