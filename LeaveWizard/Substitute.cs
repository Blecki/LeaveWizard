using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public class Substitute
    {
        public String Name;
        public int[] Matrix = new int[3];

        public override string ToString()
        {
            return String.Format("{0} [{1} {2} {3}]", 
                Name, 
                Matrix[0] == 0 ? "-" : Matrix[0].ToString(),
                Matrix[1] == 0 ? "-" : Matrix[1].ToString(),
                Matrix[2] == 0 ? "-" : Matrix[2].ToString());
        }
    }
}
