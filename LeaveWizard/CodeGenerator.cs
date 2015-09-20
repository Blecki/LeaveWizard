using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LeaveWizard
{
    public class CodeGenerator
    {
        public static String GenerateChangeCode(WeekData Start, WeekData End)
        {
            var result = new StringBuilder();

            if (Start == null)
            {
                Start = new WeekData();
                Start.Propogate(null);
            }

            var startDate = Start.SaturdayDate.ToShortDateString();
            var expectedEndDate = (Start.SaturdayDate + TimeSpan.FromDays(7)).ToShortDateString();
            var endDate = End.SaturdayDate.ToShortDateString();

            var expectedEndWeek = Start.Week + 1;
            var expectedEndPP = Start.PayPeriod;

            if (expectedEndWeek == 3)
            {
                expectedEndPP += 1;
                expectedEndWeek = 1;
            }

            if (endDate != expectedEndDate || expectedEndWeek != End.Week || expectedEndPP != End.PayPeriod || Start.Year != End.Year)
                result.Append(BaseData(End.SaturdayDate, End.Year, End.PayPeriod, End.Week));

            if (End.LeavePolicy.DaysPerSub != Start.LeavePolicy.DaysPerSub || End.LeavePolicy.AmazonRoutes != Start.LeavePolicy.AmazonRoutes)
                result.Append(Policy(End.LeavePolicy.DaysPerSub, End.LeavePolicy.AmazonRoutes));

            foreach (var sub in End.Substitutes)
            {
                var existing = Start.Substitutes.Find(s => s.Name == sub.Name && s.Matrix[0] == sub.Matrix[0] && s.Matrix[1] == sub.Matrix[1] && s.Matrix[2] == sub.Matrix[2]);
                if (existing == null)
                    result.Append(Substitute(sub));
            }

            foreach (var sub in Start.Substitutes)
            {
                var existing = End.Substitutes.Find(s => s.Name == sub.Name);
                if (existing == null)
                    result.Append(DeleteSubstitute(sub.Name));
            }

            foreach (var regular in End.Regulars.Where(r => Start.Regulars.Find(s => s.Name == r.Name && s.Route == r.Route) == null))
                result.Append(Regular(regular.Name, regular.Route));
            foreach (var regular in Start.Regulars.Where(r => End.Regulars.Find(s => s.Name == r.Name) == null))
                result.Append(DeleteRegular(regular.Name));

            for (int i = 6; i >= 0; --i)
            {
                foreach (var day in End.DailySchedules[i].ReliefDays)
                {
                    bool subAssigned = false;

                    if (day.LeaveType != "H") // H days should only exist as a result of a future holiday.
                    {
                        if (Constants.PropogatableLeaveTypes.Contains(day.LeaveType))
                        {
                            var existingPropogatableDay = Start.DailySchedules[i].ReliefDays.Find(l => l.Carrier == day.Carrier && l.LeaveType == day.LeaveType);

                            if (existingPropogatableDay == null && day.LeaveType == "J")
                            {
                                var tuple = Start.PendingJDays.Find(t => t.Item2.Carrier == day.Carrier && t.Item2.LeaveType == day.LeaveType);
                                if (tuple != null) existingPropogatableDay = tuple.Item2;
                            }

                            if (existingPropogatableDay == null)
                                result.Append(Leave(day.Carrier, i, day.LeaveType));
                            else
                            {
                                subAssigned = true;
                                if (day.Substitute != existingPropogatableDay.Substitute)
                                    result.Append(Assign(day.Carrier, day.Substitute, i));
                            }
                        }
                        else
                            result.Append(Leave(day.Carrier, i, day.LeaveType));
                    }

                    if (!subAssigned && day.Substitute != "DUMMY")
                        result.Append(Assign(day.Carrier, day.Substitute, i));
                }

                foreach (var day in Start.DailySchedules[i].ReliefDays.Where(r => Constants.PropogatableLeaveTypes.Contains(r.LeaveType)))
                {
                    var existingDay = End.DailySchedules[i].ReliefDays.Find(l => l.Carrier == day.Carrier && l.LeaveType == day.LeaveType);
                    if (existingDay == null)
                        result.Append(DeleteLeave(day.Carrier, i));
                }

                if (End.DailySchedules[i].IsHoliday)
                    result.Append(Holiday(i));

                foreach (var day in End.DailySchedules[i].DeniedLeave)
                    result.Append(DeniedLeave(day.Carrier, i, day.LeaveType));
            }

            foreach (var jDay in Start.PendingJDays)
            {
                var existingJDay = End.DailySchedules[(int)jDay.Item1].ReliefDays.Find(l => l.Carrier == jDay.Item2.Carrier && l.LeaveType == jDay.Item2.LeaveType);
                if (existingJDay == null)
                    result.Append(DeleteLeave(jDay.Item2.Carrier, (int)jDay.Item1));
            }

            return result.ToString();
        }

        public static String BaseData(DateTime SaturdayDate, int Year, int PayPeriod, int Week)
        {
            return String.Format("B\"{0}\"{1} {2} {3}\n", SaturdayDate.ToShortDateString(), Year, PayPeriod, Week);
        }

        public static String Policy(int DaysPerSub, int AmazonRoutes)
        {
            return String.Format("P{0} {1}\n", DaysPerSub, AmazonRoutes);
        }

        public static String Substitute(String Name, int[] Matrix)
        {
            return String.Format("S\"{0}\"M{1} {2} {3}\n", Name, Matrix[0], Matrix[1], Matrix[2]);
        }

        public static String Substitute(Substitute Sub)
        {
            return Substitute(Sub.Name, Sub.Matrix);
        }

        public static String DeleteSubstitute(String Name)
        {
            return String.Format("DS\"{0}\"\n", Name);
        }

        public static String Regular(String Name, int Route)
        {
            return String.Format("R\"{0}\"{1}\n", Name, Route);
        }

        public static String DeleteRegular(String Name)
        {
            return String.Format("DR\"{0}\"\n", Name);
        }

        public static String Leave(String Carrier, int Day, String LeaveType)
        {
            return String.Format("L\"{0}\"{1}{2}\n", Carrier, Constants.DayNames[Day], LeaveType);
        }

        public static String Assign(String Carrier, String Substitute, int Day)
        {
            return String.Format("A\"{0}\"\"{1}\"{2}\n", Carrier, Substitute, Constants.DayNames[Day]);
        }

        public static String DeleteLeave(String Carrier, int Day)
        {
            return String.Format("DL\"{0}\"{1}\n", Carrier, Constants.DayNames[Day]);
        }

        public static String DeniedLeave(String Carrier, int Day, String LeaveType)
        {
            return String.Format("LD\"{0}\"{1}{2}\n", Carrier, Constants.DayNames[Day], LeaveType);
        }

        public static String Holiday(int Day)
        {
            return String.Format("H{0}\n", Constants.DayNames[Day]);
        }
    }
}
