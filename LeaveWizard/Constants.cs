using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public static class Constants
    {
        public static String[] AllLeaveTypes = new String[] 
        { 
            "A",
            "S", 
            "K", 
            "J", 
            "V", 
            "X", 
            "P", 
            "W", 
            "LWOP", 
            "E", 
            "H",
            "USL",
            "UEA",
            "AWOL",
            "ULWOP"
        };

        public static String[] DayNames = new String[] { "SAT", "SUN", "MON", "TUE", "WED", "THU", "FRI" };
    }
}
