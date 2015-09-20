using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard.AnalysisReports
{
    public class CarrierDetail : LeaveAnalysis
    {
        public String Carrier { get; set; }
        public String LeaveTypes { get; set; }

        public CarrierDetail(LeaveChart Chart) : base(Chart)
        {
            this.ReportName = "Carrier Detail";
            this.Carrier = "DUMMY";
            this.LeaveTypes = "AWOL, USL, ULWOP, UEA";
            this.Summary = "Summarize a single carrier's leave usage, by date.";
        }

        private static Dictionary<String, String> MakeDictionary(String Key, String Value)
        {
            var r = new Dictionary<String, String>();
            r.Add(Key, Value);
            return r;
        }

        public override LeaveAnalysisResults Analyze(LeaveChart Data)
        {
            var leaveTypes = LeaveTypes.Split(',').Select(s => s.Trim());

            var result = new LeaveAnalysisResults();
            result.ColumnNames.Add("Date");
            result.ColumnNames.Add("Leave Type");

            foreach (var leaveEntry in Data.EnumerateLeaveWithDates(StartDate, EndDate).Where(p => p.LeaveEntry.Carrier == Carrier && leaveTypes.Contains(p.LeaveEntry.LeaveType)))
                result.Rows.Add(new LeaveAnalysisRow
                {
                    Name = leaveEntry.Date.ToShortDateString(),
                    Columns = MakeDictionary("Leave Type", leaveEntry.LeaveEntry.LeaveType)
                });

            return result;
        }
    }
}
