using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeaveWizard
{
    public partial class LeaveChart
    {
        public List<WeekData> Weeks = new List<WeekData>();
        public int CurrentWeek = 0;

        public void SaveData()
        {
            var writer = new System.IO.StreamWriter("data.txt");
            for (var i = 0; i < Weeks.Count; ++i)
            {
                if (String.IsNullOrEmpty(Weeks[i].EffectiveChanges)) continue;
                writer.Write(i);
                writer.Write(" ");
                writer.Write(Weeks[i].EffectiveChanges);
                writer.WriteLine(";");
            }

            writer.Close();
        }

        public void LoadData()
        {
            try
            {
                Weeks = new List<WeekData>();
                CurrentWeek = 0;

                if (!System.IO.File.Exists("data.txt"))
                {
                    Weeks.Add(new WeekData());
                    return;
                }

                var text = System.IO.File.ReadAllText("data.txt").Trim();
                var stream = new StringIterator(text);

                while (!stream.AtEnd)
                {
                    var weekId = 0;
                    var weekData = "";
                    SkipWhitespace(stream);
                    ParseWeek(stream, out weekId, out weekData);

                    while (weekId >= Weeks.Count) Weeks.Add(new WeekData());
                    Weeks[weekId].EffectiveChanges = weekData;
                }
            }
            catch (Exception e)
            {
                System.Windows.MessageBox.Show(String.Format("An error has occured while loading saved data.\n{0}\nLoading will be aborted to preserve your existing data. To discard your saved data, delete 'data.txt' found in the folder where you installed Leave Wizard. It is likely that your existing data has been corrupted, but it might be possible to save it.", e.Message), "Critical error!", System.Windows.MessageBoxButton.OK);
                throw e;
            }
        }

        private void SkipWhitespace(StringIterator Iter)
        {
            while (!Iter.AtEnd && (Iter.Next == ' ' || Iter.Next == '\n' || Iter.Next == '\t' || Iter.Next == '\r'))
                Iter.Advance();
        }

        private static void ParseWeek(StringIterator Stream, out int WeekId, out String WeekData)
        {
            WeekId = 0;
            WeekData = "";

            var idString = "";
            while (!Stream.AtEnd && Stream.Next >= '0' && Stream.Next <= '9')
            {
                idString += (char)Stream.Next;
                Stream.Advance();
            }

            if (String.IsNullOrEmpty(idString)) return;

            var dataString = "";
            while (!Stream.AtEnd && Stream.Next != ';')
            {
                dataString += (char)Stream.Next;
                Stream.Advance();
            }

            Stream.Advance();

            WeekId = Convert.ToInt32(idString);
            WeekData = dataString;
        }
    }

}
