using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public class LeaveEntry
    {
        public String LeaveType = "L";
        public String Carrier = "DUMMY";
        public String Substitute = "DUMMY";

        public LeaveEntry CloneForNewWeek()
        {
            return new LeaveEntry { LeaveType = this.LeaveType, Carrier = this.Carrier, Substitute = this.Substitute };
        }
    }
}
