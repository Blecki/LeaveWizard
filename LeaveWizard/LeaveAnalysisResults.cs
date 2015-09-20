using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class LeaveAnalysisRow
    {
        public String Name;
        public Dictionary<String, String> Columns = new Dictionary<String, String>();
    }

    public class LeaveAnalysisResults
    {
        public List<String> ColumnNames = new List<string>();
        public List<LeaveAnalysisRow> Rows = new List<LeaveAnalysisRow>();

        public void SortRowsAlphabetically()
        {
            Rows = new List<LeaveAnalysisRow>(Rows.OrderBy(r => r.Name));
        }
    }
}
