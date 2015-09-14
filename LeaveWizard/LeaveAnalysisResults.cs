using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class LeaveAnalysisRow
    {
        public String Name;
        public Dictionary<String, int> Columns = new Dictionary<string, int>();
    }

    public class LeaveAnalysisResults
    {
        public List<String> ColumnNames = new List<string>();
        public List<LeaveAnalysisRow> Rows = new List<LeaveAnalysisRow>();
    }
}
