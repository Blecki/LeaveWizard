using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class LeaveAnalysis
    {
        public virtual String ReportName { get { return "Unimplemented Report"; } }

        public virtual LeaveAnalysisResults Analyze(LeaveChart Data)
        {
            throw new NotImplementedException();
        }
    }
}
