﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public class SubRow
    {
        public String Name;
        public String[] Schedule = new String[7];
    }

    public class SubSchedule
    {
        public List<SubRow> Subs;
        public List<List<String>> OpenRoutes;
    }

    public partial class LeaveChart
    {
        public static SubSchedule GenerateSubSchedule(WeekData Week)
        {
            var result = new SubSchedule();
            result.Subs = new List<SubRow>();
            result.OpenRoutes = new List<List<String>>();
            for (int i = 0; i < 7; ++i) result.OpenRoutes.Add(new List<String>());

            for (var day = 0; day < 7; ++day)
                foreach (var leaveEntry in Week.DailySchedules[day].ReliefDays)
                {
                    var subName = "";
                    var data = "";
                    var regular = Week.Regulars.Find(r => r.Name == leaveEntry.Carrier);
                    if (regular != null)
                    {
                        if (Week.DailySchedules[day].IsHoliday) continue;
                        if ("R35V".Contains(leaveEntry.LeaveType)) continue; //Don't put regulars on schedule for working relief day.
                        subName = leaveEntry.Substitute;
                        data = regular.Route.ToString();

                    }
                    else
                    {
                        subName = leaveEntry.Carrier;
                        data = leaveEntry.LeaveType.ToString();
                    }

                    if (subName == "DUMMY")
                        result.OpenRoutes[day].Add(data);
                    else
                    {
                        var sub = result.Subs.Find(r => r.Name == subName);
                        if (sub == null)
                        {
                            sub = new SubRow { Name = subName };
                            result.Subs.Add(sub);
                        }

                        sub.Schedule[day] = data;
                    }
                }

            result.Subs = new List<SubRow>(result.Subs.OrderBy(r => r.Name));
            return result;
        }
       
    }
}