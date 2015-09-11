using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class LeaveAnalysisEntry
    {
        public String Carrier;
        public Dictionary<String, int> LeaveUsage = new Dictionary<string, int>();
    }

    public class LeaveAnalysisResults : List<LeaveAnalysisEntry>
    {
    }
}
