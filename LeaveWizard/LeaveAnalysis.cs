using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class LeaveAnalysis
    {
        public String ReportName { get; protected set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public LeaveAnalysis(LeaveChart Chart)
        {
            this.ReportName = "Unimplemented Report";
            StartDate = Chart.Weeks[0].SaturdayDate;
            EndDate = DateTime.Now.Date;
        }

        public virtual LeaveAnalysisResults Analyze(LeaveChart Data)
        {
            throw new NotImplementedException();
        }
    }
}
