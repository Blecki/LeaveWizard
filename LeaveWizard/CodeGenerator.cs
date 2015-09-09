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
                result.AppendFormat("B\"{0}\"{1} {2} {3}\n", End.SaturdayDate.ToShortDateString(), End.Year, End.PayPeriod, End.Week);

            if (End.LeavePolicy.DaysPerSub != Start.LeavePolicy.DaysPerSub || End.LeavePolicy.AmazonRoutes != Start.LeavePolicy.AmazonRoutes)
                result.AppendFormat("P{0} {1}\n", End.LeavePolicy.DaysPerSub, End.LeavePolicy.AmazonRoutes);

            foreach (var sub in End.Substitutes)
            {
                var existing = Start.Substitutes.Find(s => s.Name == sub.Name && s.Matrix[0] == sub.Matrix[0] && s.Matrix[1] == sub.Matrix[1] && s.Matrix[2] == sub.Matrix[2]);
                if (existing == null)
                    result.AppendFormat("S\"{0}\"M{1} {2} {3}\n", sub.Name, sub.Matrix[0], sub.Matrix[1], sub.Matrix[2]);
            }

            foreach (var sub in Start.Substitutes)
            {
                var existing = End.Substitutes.Find(s => s.Name == sub.Name);
                if (existing == null)
                    result.AppendFormat("DS\"{0}\"\n", sub.Name);
            }

            foreach (var regular in End.Regulars.Where(r => Start.Regulars.Find(s => s.Name == r.Name && s.Route == r.Route) == null))
                result.AppendFormat("R\"{0}\"{1}\n", regular.Name, regular.Route);
            foreach (var regular in Start.Regulars.Where(r => End.Regulars.Find(s => s.Name == r.Name) == null))
                result.AppendFormat("DR\"{0}\"\n", regular.Name);

            for (int i = 6; i >= 0; --i)
            {
                foreach (var day in End.DailySchedules[i].ReliefDays)
                {
                    bool subAssigned = false;

                    if (day.LeaveType != "H") // H days should only exist as a result of a future holiday.
                    {
                        if (DailySchedule.PropogatableLeaveTypes.Contains(day.LeaveType))
                        {
                            var existingPropogatableDay = Start.DailySchedules[i].ReliefDays.Find(l => l.Carrier == day.Carrier && l.LeaveType == day.LeaveType);

                            if (existingPropogatableDay == null && day.LeaveType == "J")
                            {
                                var tuple = Start.PendingJDays.Find(t => t.Item2.Carrier == day.Carrier && t.Item2.LeaveType == day.LeaveType);
                                if (tuple != null) existingPropogatableDay = tuple.Item2;
                            }

                            if (existingPropogatableDay == null)
                                result.AppendFormat("L\"{0}\"{1}{2}\n", day.Carrier, Constants.DayNames[i], day.LeaveType);
                            else
                            {
                                subAssigned = true;
                                if (day.Substitute != existingPropogatableDay.Substitute)
                                    result.AppendFormat("A\"{0}\"\"{1}\"{2}\n", day.Carrier, day.Substitute, Constants.DayNames[i]);
                            }
                        }
                        else
                            result.AppendFormat("L\"{0}\"{1}{2}\n", day.Carrier, Constants.DayNames[i], day.LeaveType);
                    }

                    if (!subAssigned && day.Substitute != "DUMMY")
                        result.AppendFormat("A\"{0}\"\"{1}\"{2}\n", day.Carrier, day.Substitute, Constants.DayNames[i]);
                }

                foreach (var day in Start.DailySchedules[i].ReliefDays.Where(r => DailySchedule.PropogatableLeaveTypes.Contains(r.LeaveType)))
                {
                    var existingDay = End.DailySchedules[i].ReliefDays.Find(l => l.Carrier == day.Carrier && l.LeaveType == day.LeaveType);
                    if (existingDay == null)
                        result.AppendFormat("DL\"{0}\"{1}\n", day.Carrier, Constants.DayNames[i]);
                }

                if (End.DailySchedules[i].IsHoliday)
                    result.AppendFormat("H{0}\n", Constants.DayNames[i]);

                foreach (var day in End.DailySchedules[i].DeniedLeave)
                {
                    result.AppendFormat("LD\"{0}\"{1}{2}\n", day.Carrier, Constants.DayNames[i], day.LeaveType);
                }
            }

            foreach (var jDay in Start.PendingJDays)
            {
                var existingJDay = End.DailySchedules[(int)jDay.Item1].ReliefDays.Find(l => l.Carrier == jDay.Item2.Carrier && l.LeaveType == jDay.Item2.LeaveType);
                if (existingJDay == null)
                    result.AppendFormat("DL\"{0}\"{1}\n", jDay.Item2.Carrier, Constants.DayNames[(int)jDay.Item1]);
            }

            return result.ToString();
        }
    }
}
