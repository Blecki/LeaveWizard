using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public class RegularCarrier
    {
        public String Name = "DUMMY";
        public int Route = 0;

        public override string ToString()
        {
            return String.Format("R{0:000} {1}", Route, Name);
        }
    }
}
