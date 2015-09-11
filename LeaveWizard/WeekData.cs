using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public class LeavePolicy
    {
        public int DaysPerSub;
        public int AmazonRoutes;
    }

    public class WeekData
    {
        public int Year;
        public int PayPeriod;
        public int Week;

        public LeavePolicy LeavePolicy;

        public DateTime SaturdayDate;
        public List<Substitute> Substitutes = new List<Substitute>();
        public List<RegularCarrier> Regulars = new List<RegularCarrier>();
        public DailySchedule[] DailySchedules = new DailySchedule[7];
        public List<Tuple<DayOfWeek, LeaveEntry>> PendingJDays = new List<Tuple<DayOfWeek, LeaveEntry>>();
        public String EffectiveChanges;

        public WeekData PreviousWeek;

        public virtual void ApplyChanges() { }

        public WeekData()
        {
            for (int i = 0; i < 7; ++i) DailySchedules[i] = new DailySchedule();
            LeavePolicy = new LeavePolicy { DaysPerSub = 5, AmazonRoutes = 0 };
        }

        public int SumLeaveDays()
        {
            return DailySchedules.Sum(ds => ds.IsHoliday ? LeavePolicy.AmazonRoutes : ds.ReliefDays.Count) + LeavePolicy.AmazonRoutes;
        }

        public void Propogate(WeekData PreviousWeek)
        {
            this.PreviousWeek = PreviousWeek;

            if (PreviousWeek == null)
            {
                SaturdayDate = DateTime.FromOADate(0);
                PayPeriod = 0;
                Week = 1;
                Year = 2015;
                Substitutes = new List<Substitute>();
                Regulars = new List<RegularCarrier>();
                DailySchedules = new DailySchedule[7];
                for (int i = 0; i < 7; ++i) DailySchedules[i] = new DailySchedule();
                PendingJDays = new List<Tuple<DayOfWeek, LeaveEntry>>();
                LeavePolicy = new LeavePolicy { DaysPerSub = 5, AmazonRoutes = 0 };
            }
            else
            {
                LeavePolicy = PreviousWeek.LeavePolicy;
                Year = PreviousWeek.Year;
                Week = PreviousWeek.Week + 1;
                if (Week == 3)
                {
                    PayPeriod = PreviousWeek.PayPeriod + 1;
                    Week = 1;
                }
                else
                    PayPeriod = PreviousWeek.PayPeriod;

                SaturdayDate = PreviousWeek.SaturdayDate + TimeSpan.FromDays(7);

                Substitutes = new List<Substitute>(PreviousWeek.Substitutes);
                Regulars = new List<RegularCarrier>(PreviousWeek.Regulars);

                PendingJDays = new List<Tuple<DayOfWeek, LeaveEntry>>();
                DailySchedules = PreviousWeek.DailySchedules.Select(ds => ds.CloneForNewWeek()).ToArray();
                for ( var day = 0; day < 7; ++day)
                {
                    foreach (var rd in DailySchedules[day].ReliefDays)
                        if (rd.LeaveType == "J") PendingJDays.Add(Tuple.Create((DayOfWeek)day, rd));
                    DailySchedules[day].ReliefDays.RemoveAll(rd => rd.LeaveType == "J");
                }

                if (PreviousWeek.PendingJDays != null)
                    foreach (var pendingJ in PreviousWeek.PendingJDays)
                        DailySchedules[(int)pendingJ.Item1].ReliefDays.Add(pendingJ.Item2);
            }

            if (String.IsNullOrEmpty(EffectiveChanges)) EffectiveChanges = "";
            var iter = new StringIterator(EffectiveChanges);

            try
            {
                while (!iter.AtEnd)
                {
                    SkipWhitespace(iter);
                    if (iter.Next == 'S')
                    {
                        var sub = ParseSub(iter);
                        var existing = Substitutes.Find(s => s.Name == sub.Name);
                        if (existing != null) Substitutes.Remove(existing);
                        Substitutes.Add(sub);
                    }
                    else if (iter.Next == 'R')
                    {
                        var regular = ParseRegular(iter);
                        var existing = Regulars.Find(r => r.Name == regular.Name);
                        if (existing != null) Regulars.Remove(existing);
                        Regulars.Add(regular);
                    }
                    else if (iter.Next == 'L')
                    {
                        bool denied = false;
                        iter.Advance();
                        SkipWhitespace(iter);
                        if (iter.Next == 'D')
                        {
                            iter.Advance();
                            SkipWhitespace(iter);
                            denied = true;
                        }
                        var name = ParseName(iter);
                        SkipWhitespace(iter);
                        var day = ParseDay(iter);
                        SkipWhitespace(iter);
                        var type = ParseLeave(iter);
                        iter.Advance();
                        var leave = new LeaveEntry { Carrier = name, LeaveType = type, Substitute = "DUMMY" };
                        if (denied)
                            DailySchedules[(int)day].DeniedLeave.Add(leave);
                        else
                            DailySchedules[(int)day].ReliefDays.Add(leave);
                    }
                    else if (iter.Next == 'B')
                    {
                        iter.Advance();
                        SkipWhitespace(iter);
                        var dateString = ParseName(iter);
                        SkipWhitespace(iter);
                        var year = ParseRoute(iter);
                        SkipWhitespace(iter);
                        var payPeriod = ParseRoute(iter);
                        SkipWhitespace(iter);
                        var week = ParseRoute(iter);

                        this.SaturdayDate = Convert.ToDateTime(dateString);
                        this.Year = year;
                        this.PayPeriod = payPeriod;
                        this.Week = week;
                    }
                    else if (iter.Next == 'P')
                    {
                        iter.Advance();
                        SkipWhitespace(iter);
                        var daysPerSub = ParseRoute(iter);
                        SkipWhitespace(iter);
                        var amazonRoutes = ParseRoute(iter);
                        LeavePolicy = new LeavePolicy { DaysPerSub = daysPerSub, AmazonRoutes = amazonRoutes };
                    }
                    else if (iter.Next == 'A')
                    {
                        iter.Advance();
                        SkipWhitespace(iter);
                        var regular = ParseName(iter);
                        SkipWhitespace(iter);
                        var sub = ParseName(iter);
                        SkipWhitespace(iter);
                        var day = ParseDay(iter);
                        if (sub != "DUMMY")
                            foreach (var reliefDay in DailySchedules[(int)day].ReliefDays)
                                if (reliefDay.Substitute == sub) reliefDay.Substitute = "DUMMY";
                        foreach (var reliefDay in DailySchedules[(int)day].ReliefDays)
                            if (reliefDay.Carrier == regular) reliefDay.Substitute = sub;
                    }
                    else if (iter.Next == 'H')
                    {
                        iter.Advance();
                        SkipWhitespace(iter);
                        var day = ParseDay(iter);
                        Holiday(day);
                    }
                    else if (iter.Next == 'D')
                    {
                        iter.Advance();
                        if (iter.Next == 'S')
                        {
                            iter.Advance();
                            SkipWhitespace(iter);
                            var name = ParseName(iter);
                            Substitutes.RemoveAll(s => s.Name == name);
                        }
                        else if (iter.Next == 'R')
                        {
                            iter.Advance();
                            SkipWhitespace(iter);
                            var name = ParseName(iter);
                            Regulars.RemoveAll(r => r.Name == name);
                        }
                        else if (iter.Next == 'L')
                        {
                            iter.Advance();
                            SkipWhitespace(iter);
                            var name = ParseName(iter);
                            SkipWhitespace(iter);
                            var day = ParseDay(iter);
                            DailySchedules[(int)day].ReliefDays.RemoveAll(r => r.Carrier == name);
                        }
                        else if (iter.Next == 'D')
                        {
                            iter.Advance();
                            SkipWhitespace(iter);
                            var name = ParseName(iter);
                            SkipWhitespace(iter);
                            var day = ParseDay(iter);
                            DailySchedules[(int)day].DeniedLeave.RemoveAll(r => r.Carrier == name);
                        }
                        else if (iter.Next == 'H')
                        {
                            iter.Advance();
                            SkipWhitespace(iter);
                            var day = ParseDay(iter);
                            ClearHoliday(day);
                        }
                    }
                    else
                    {
                        iter.Advance();
                    }
                }
            }
            catch (Exception e)
            {
               
            }

            foreach (var ds in DailySchedules) ds.ReliefDays = new List<LeaveEntry>(ds.ReliefDays.OrderBy(rd => rd.Carrier));

            Substitutes = new List<Substitute>(Substitutes.OrderBy(s => s.Name));
            Regulars = new List<RegularCarrier>(Regulars.OrderBy(r => r.Route));
        }

        public void ApplyChange(WeekData PreviousWeek, String Command)
        {
            EffectiveChanges += Command;
            Propogate(PreviousWeek);
            EffectiveChanges = CodeGenerator.GenerateChangeCode(PreviousWeek, this);
        }

        #region Parsing

        private DayOfWeek ParseDay(StringIterator Iter)
        {
            var dayName = "";
            for (var i = 0; i < 3; ++i)
            {
                if (Iter.AtEnd) continue;
                dayName += (char)Iter.Next;
                Iter.Advance();
            }

            for (int i = 0; i < Constants.DayNames.Length; ++i)
            {
                if (dayName == Constants.DayNames[i]) return (DayOfWeek)i;
            }

            return DayOfWeek.Saturday;
        }

        private RegularCarrier ParseRegular(StringIterator Iter)
        {
            var regular = new RegularCarrier();
            Iter.Advance();
            SkipWhitespace(Iter);
            regular.Name = ParseName(Iter);
            SkipWhitespace(Iter);
            regular.Route = ParseRoute(Iter);
            return regular;
        }

        private Substitute ParseSub(StringIterator Iter)
        {
            var sub = new Substitute();
            Iter.Advance();
            SkipWhitespace(Iter);
            sub.Name = ParseName(Iter);
            SkipWhitespace(Iter);
            if (Iter.Next == 'M')
            {
                Iter.Advance();
                SkipWhitespace(Iter);
                sub.Matrix[0] = ParseRoute(Iter);
                SkipWhitespace(Iter);
                sub.Matrix[1] = ParseRoute(Iter);
                SkipWhitespace(Iter);
                sub.Matrix[2] = ParseRoute(Iter);
            }

            return sub;
        }

        private void SkipWhitespace(StringIterator Iter)
        {
            while (!Iter.AtEnd && (Iter.Next == ' ' || Iter.Next == '\n' || Iter.Next == '\t' || Iter.Next == '\r'))
                Iter.Advance();
        }

        private bool IsWhitespace(StringIterator Iter)
        {
            return (Iter.Next == ' ' || Iter.Next == '\n' || Iter.Next == '\t' || Iter.Next == '\r');
        }

        private int ParseRoute(StringIterator Iter)
        {
            var numberString = "";
            while (!Iter.AtEnd && Iter.Next >= '0' && Iter.Next <= '9')
            {
                numberString += (char)Iter.Next;
                Iter.Advance();
            }
            return Convert.ToInt32(numberString);
        }

        private string ParseName(StringIterator Iter)
        {
            var nameString = "";
            Iter.Advance();
            while (!Iter.AtEnd && Iter.Next != '"')
            {
                nameString += (char)Iter.Next;
                Iter.Advance();
            }
            Iter.Advance();
            return nameString;
        }

        private string ParseLeave(StringIterator Iter)
        {
            var leave = "";
            while (!Iter.AtEnd && !IsWhitespace(Iter))
            {
                leave += (char)Iter.Next;
                Iter.Advance();
            }
            return leave;
        }

        #endregion

        #region Mutator Funcs

        public void Holiday(DayOfWeek Day)
        {
            if (DailySchedules[(int)Day].IsHoliday) return; //It's already a holiday.
            DailySchedules[(int)Day].IsHoliday = true;
            DailySchedule previousDay = null;
            if (Day == DayOfWeek.Saturday)
            {
                if (PreviousWeek == null) return;
                previousDay = PreviousWeek.DailySchedules[(int)DayOfWeek.Friday];
            }
            else if (Day == DayOfWeek.Monday) //Skip Sundays
                previousDay = DailySchedules[(int)DayOfWeek.Saturday];
            else
                previousDay = DailySchedules[(int)Day - 1];

            foreach (var reliefDay in DailySchedules[(int)Day].ReliefDays.Where(rd => rd.LeaveType == "K" || rd.LeaveType == "J"))
                previousDay.ReliefDays.Add(new LeaveEntry { LeaveType = "H", Carrier = reliefDay.Carrier });
        }

        public void ClearHoliday(DayOfWeek Day)
        {
            if (!DailySchedules[(int)Day].IsHoliday) return; //It's already not a holiday.
            DailySchedules[(int)Day].IsHoliday = false;
            DailySchedule previousDay = null;
            if (Day == DayOfWeek.Saturday)
            {
                if (PreviousWeek == null) return;
                previousDay = PreviousWeek.DailySchedules[5];
            }
            else if (Day == DayOfWeek.Monday) //Skip Sundays
                previousDay = DailySchedules[(int)DayOfWeek.Saturday];
            else
                previousDay = DailySchedules[(int)Day - 1];

            previousDay.ReliefDays.RemoveAll(d => d.LeaveType == "H");
        }


        #endregion
    }
}
