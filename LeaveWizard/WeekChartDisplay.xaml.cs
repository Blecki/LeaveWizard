using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LeaveWizard
{
    /// <summary>
    /// Interaction logic for WeekChartDisplay.xaml
    /// </summary>
    public partial class WeekChartDisplay : UserControl
    {
        public WeekData CurrentWeek = null;
        public Action<String> ApplyAction = null;
        private TextBlock MouseCoordBlock;
        private int MouseHiliteRow = -1;
        private Brush EvenRowBackground = new SolidColorBrush(Colors.LightGray);
        private Brush OddRowBackground = new SolidColorBrush(Colors.White);
        private Brush MouseRowBackground = new SolidColorBrush(Colors.PaleGoldenrod);

        public WeekChartDisplay()
        {
            InitializeComponent();
        }

        public void AddCell(Grid To, String Data, int Column, int Row, String Tooltip, Action OnClick = null)
        {
            var text = new TextBlock();
            text.Text = "| " + Data;
            if (!String.IsNullOrEmpty(Tooltip)) text.ToolTip = Tooltip;
            if (OnClick != null) text.MouseDown += (sender, args) => OnClick();
            if (Row % 2 == 0) text.Background = EvenRowBackground;
            else text.Background = OddRowBackground;
            Grid.SetColumn(text, Column);
            Grid.SetRow(text, Row);
            To.Children.Add(text);
        }

        public enum LeaveCellType
        {
            Regular,
            Sub,
            Denied,
            Sunday
        }

        public void AddLeaveEntryCell(LeaveEntry Entry, LeaveCellType Type, int Column, int Row)
        {
            if (Type == LeaveCellType.Regular)
            {
                AddToCell(MainGrid, Column, Row, new TextBlock
                {
                    Text =  String.Format("| {0} {1}", Entry.LeaveType, Entry.Substitute),
                    ToolTip = "Click to schedule substitute",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                }).MouseDown += (sender, args) =>
                    {
                        var subSelector = SimpleSelector.Show("Select Substitute", EnumerateAvailableSubs(Entry, Column - 2));
                        if (subSelector.SelectionMade)
                        {
                            ApplyAction(String.Format("A\"{0}\"\"{1}\"{2} ",
                                Entry.Carrier, subSelector.SelectedItem, Constants.DayNames[Column - 2]));
                        }
                    };
            }
            else if (Type == LeaveCellType.Sunday)
            {
                AddToCell(MainGrid, Column, Row, new TextBlock
                {
                    Text = String.Format("| {0} {1}", Entry.Carrier, Entry.Substitute),
                    ToolTip = "Click to schedule substitute",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch
                }).MouseDown += (sender, args) =>
                {
                    var subSelector = SimpleSelector.Show("Select Substitute", EnumerateAvailableSubs(Entry, Column - 2));
                    if (subSelector.SelectionMade)
                    {
                        ApplyAction(String.Format("A\"{0}\"\"{1}\"{2} ",
                            Entry.Carrier, subSelector.SelectedItem, Constants.DayNames[Column - 2]));
                    }
                };
            }
            else if (Type == LeaveCellType.Sub || Type == LeaveCellType.Denied)
            {
                AddToCell(MainGrid, Column, Row, new TextBlock
                {
                    Text = String.Format("|  {0} {1}", Entry.LeaveType, Entry.Carrier),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                });
            }

            if (Type != LeaveCellType.Sunday)
            {
                AddToCell(MainGrid, Column, Row, new TextBlock
                    {
                        Text = "[del] ",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        ToolTip = "Click to delete leave"
                    }).MouseDown += (sender, args) => ApplyAction(String.Format("D{2}\"{0}\"{1} ", Entry.Carrier, Constants.DayNames[Column - 2], (Type == LeaveCellType.Denied ? "D" : "L")));
            }
        }

        private TextBlock AddToCell(Grid To, int Column, int Row, TextBlock Control)
        {
            if (Row % 2 == 0) Control.Background = EvenRowBackground;
            Grid.SetColumn(Control, Column);
            Grid.SetRow(Control, Row);
            To.Children.Add(Control);
            return Control;
        }            

        private String OffsetDateString(int Day)
        {
            return (CurrentWeek.SaturdayDate + TimeSpan.FromDays(Day)).ToShortDateString();
        }

        public IEnumerable<int> EnumerateFullColumnHeights(WeekData Week)
        {
            yield return Week.Substitutes.Count + 1;
            yield return Week.Regulars.Count + 1;
            yield return EnumerateLeaveColumnHeights(Week).Max() + EnumerateDeniedLeaveCounts(Week).Max();
        }

        public IEnumerable<int> EnumerateLeaveColumnHeights(WeekData Week)
        {
            foreach (var day in Week.DailySchedules)
                yield return day.ReliefDays.Where(r => Week.Regulars.Find(c => c.Name == r.Carrier) == null).Count() + Week.Regulars.Count;
        }

        public IEnumerable<int> EnumerateDeniedLeaveCounts(WeekData Week)
        {
            foreach (var day in Week.DailySchedules)
                yield return day.DeniedLeave.Count();
        }

        public IEnumerable<String> EnumerateAvailableSubs(LeaveEntry Leave, int Day)
        {
            if (Leave.LeaveType == "SUNDAY")
            {
                foreach (var sub in CurrentWeek.Substitutes)
                {
                    var foundDay = CurrentWeek.DailySchedules[Day].ReliefDays.Find(d => d.Substitute == sub.Name);
                    if (foundDay != null && foundDay.LeaveType == "SUNDAY") continue;
                    if (CurrentWeek.DailySchedules[Day].ReliefDays.Find(d => d.Carrier == sub.Name) != null) continue;
                    yield return sub.Name;
                }
            }
            else
            {
                if (Leave.LeaveType == "K" || Leave.LeaveType == "J")
                {
                    yield return "3 - " + Leave.Carrier;
                    yield return "5 - " + Leave.Carrier;
                    yield return "R - " + Leave.Carrier;
                }

                if (Leave.LeaveType == "H") yield return "V - " + Leave.Carrier;

                yield return "DUMMY";

                foreach (var sub in CurrentWeek.Substitutes.Where(s => CurrentWeek.DailySchedules[Day].ReliefDays.Find(d => d.Substitute == s.Name) == null).Where(s => CurrentWeek.DailySchedules[Day].ReliefDays.Find(d => d.Carrier == s.Name) == null))
                    yield return sub.Name;
            }
        }

        public IEnumerable<String> EnumerateAllCarriers()
        {
            foreach (var regular in CurrentWeek.Regulars)
                yield return regular.Name;
            foreach (var sub in CurrentWeek.Substitutes)
                yield return sub.Name;
        }

        public void DisplayWeek(WeekData Week)
        {
            CurrentWeek = Week;
            TopGrid.Children.Clear();
            MainGrid.Children.Clear();
            MainGrid.RowDefinitions.Clear();

            MouseCoordBlock = AddToCell(TopGrid, 0, 1, new TextBlock { Text = "----" });
            
            if (CurrentWeek == null) return;

            var maxRows = EnumerateFullColumnHeights(Week).Max() + 4;
            var approvedLeaveRows = EnumerateLeaveColumnHeights(Week).Max() + 2;

            MainGrid.Height = 20 * maxRows;

            for (var i = 0; i < maxRows; ++i)
                MainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });

            AddCell(TopGrid, "Substitutes", 0, 0, "All available substitutes");
            AddCell(TopGrid, "", 0, 1, null);
            for (var i = 0; i < maxRows; ++i)
            {
                if (i > Week.Substitutes.Count)
                    AddCell(MainGrid, "", 0, i, "", null);
                else if (i == Week.Substitutes.Count)
                    AddCell(MainGrid, "+ new substitute", 0, i, "Click to add new substitute", () =>
                        {
                            var creator = CreateSub.Show();
                            if (creator.FinishedInput)
                                ApplyAction(String.Format("S\"{0}\" ", creator.Name));
                        });
                else if (i < Week.Substitutes.Count)
                {
                    var lambdaSub = Week.Substitutes[i];
                    AddCell(MainGrid, Week.Substitutes[i].ToString(), 0, i, "Click to modify matrix assignments", () =>
                        {
                            var matrixEditor = ModifyMatrix.Show(lambdaSub);
                            if (matrixEditor.FinishedInput)
                                ApplyAction(String.Format("S\"{0}\"M{1} {2} {3} ", lambdaSub.Name, matrixEditor.Choices[0], matrixEditor.Choices[1], matrixEditor.Choices[2]));
                        });

                    AddToCell(MainGrid, 0, i, new TextBlock
                    {
                        Text = "[del] ",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        ToolTip = "Click to delete substitute"
                    }).MouseDown += (sender, args) => ApplyAction(String.Format("DS\"{0}\" ", lambdaSub.Name));
                }
            }

            AddCell(TopGrid, "Routes", 1, 0, "All routes");
            AddCell(TopGrid, "", 1, 1, null);
            for (var i = 0; i < maxRows; ++i)
            {
                if (i > Week.Regulars.Count)
                    AddCell(MainGrid, "", 1, i, "", null);
                else if (i >= Week.Regulars.Count)
                    AddCell(MainGrid, "+ new regular/route", 1, i, "Click to add regular", () =>
                    {
                        var creator = CreateRegular.Show();
                        if (creator.FinishedInput)
                            ApplyAction(String.Format("R\"{0}\"{1} ", creator.Name, creator.Route));
                    });
                else
                {
                    var lambdaR = Week.Regulars[i];
                    AddCell(MainGrid, lambdaR.ToString(), 1, i, null);

                    AddToCell(MainGrid, 1, i, new TextBlock
                    {
                        Text = "[del] ",
                        HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                        ToolTip = "Click to delete regular / route"
                    }).MouseDown += (sender, args) => ApplyAction(String.Format("DR\"{0}\" ", lambdaR.Name));
                }
            }

            for (var x = 2; x < 9; ++x)
            {
                var currentDay = Week.DailySchedules[x - 2];
                var dayIndex = x - 2;
                AddCell(TopGrid, ((DayOfWeek)dayIndex).ToString() + " - " + OffsetDateString(dayIndex) + (currentDay.IsHoliday ? " - HOLIDAY" : ""), x, 0, null);
                
                var localReliefDays = new List<LeaveEntry>(Week.DailySchedules[dayIndex].ReliefDays);

                if (localReliefDays.Count >= Week.Substitutes.Count)
                    AddCell(TopGrid, "FULL", x, 1, null);
                else
                    AddCell(TopGrid, String.Format("SUBS:{0}", Week.Substitutes.Count - localReliefDays.Count), x, 1, null);

                AddToCell(TopGrid, x, 1, new TextBlock
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
                    Text = (currentDay.IsHoliday ? "[clear holiday]" : "[make holiday]"),
                }).MouseDown += (sender, args) =>
                    {
                        if (currentDay.IsHoliday) ApplyAction(String.Format("DH{0} ", Constants.DayNames[dayIndex]));
                        else ApplyAction(String.Format("H{0} ", Constants.DayNames[dayIndex]));
                    };
                
                var rowIndex = 0;

                if (dayIndex == 1 || currentDay.IsHoliday)
                {
                    var sundayRoutes = new List<LeaveEntry>(localReliefDays.Where(rd => rd.LeaveType == "SUNDAY"));
                    foreach (var sundayRoute in sundayRoutes)
                    {
                        localReliefDays.Remove(sundayRoute);
                        AddLeaveEntryCell(sundayRoute, LeaveCellType.Sunday, x, rowIndex);
                        rowIndex += 1;
                    }

                    while (rowIndex < CurrentWeek.Regulars.Count)
                    {
                        AddCell(MainGrid, "", x, rowIndex, null);
                        rowIndex += 1;
                    }

                    foreach (var regular in Week.Regulars)
                    {
                        var reliefDay = localReliefDays.Find(r => r.Carrier == regular.Name);
                        if (reliefDay != null)
                            localReliefDays.Remove(reliefDay);
                    }
                }
                else
                {
                    foreach (var regular in Week.Regulars)
                    {
                        var reliefDay = localReliefDays.Find(r => r.Carrier == regular.Name);
                        if (reliefDay == null)
                            AddCell(MainGrid, "", x, rowIndex, "Click to add leave", () =>
                                {
                                    var leaveSelector = SimpleSelector.Show("Select leave type", Constants.AllLeaveTypes.Select(c => (object)c));
                                    if (leaveSelector.SelectionMade)
                                        ApplyAction(String.Format("L\"{0}\"{1}{2} ",
                                            regular.Name, Constants.DayNames[dayIndex], leaveSelector.SelectedItem));
                                });
                        else
                        {
                            AddLeaveEntryCell(reliefDay, LeaveCellType.Regular, x, rowIndex);
                            localReliefDays.Remove(reliefDay);
                        }

                        rowIndex += 1;
                    }
                }

                foreach (var rDay in localReliefDays)
                {
                    AddLeaveEntryCell(rDay, LeaveCellType.Sub, x, rowIndex);
                    rowIndex += 1;
                }

                AddCell(MainGrid, "+ substitute leave", x, rowIndex, "Add leave for a substitute", () =>
                    {
                        var leaveSelector = SimpleSelector.Show("Select leave type", Constants.AllLeaveTypes.Select(c => (object)c));
                        if (leaveSelector.SelectionMade)
                        {
                            var carrierSelector = SimpleSelector.Show("Select carrier", EnumerateAllCarriers().Select(c => (object)c));
                            if (carrierSelector.SelectionMade)
                                ApplyAction(String.Format("L\"{0}\"{1}{2} ",
                                    carrierSelector.SelectedItem, Constants.DayNames[dayIndex], leaveSelector.SelectedItem));
                        }
                    });
                rowIndex += 1;

                while (rowIndex < approvedLeaveRows)
                {
                    AddCell(MainGrid, "", x, rowIndex, null);
                    rowIndex += 1;
                }

                AddCell(MainGrid, "DENIED LEAVE", x, rowIndex, null);
                rowIndex += 1;

                foreach (var rDay in CurrentWeek.DailySchedules[dayIndex].DeniedLeave)
                {
                    AddLeaveEntryCell(rDay, LeaveCellType.Denied, x, rowIndex);
                    rowIndex += 1;
                }

                AddCell(MainGrid, "+ denied leave", x, rowIndex, "Add denied leave", () =>
                    {
                        var leaveSelector = SimpleSelector.Show("Select leave type", Constants.AllLeaveTypes.Select(c => (object)c));
                        if (leaveSelector.SelectionMade)
                        {
                            var carrierSelector = SimpleSelector.Show("Select carrier", EnumerateAllCarriers().Select(c => (object)c));
                            if (carrierSelector.SelectionMade)
                                ApplyAction(String.Format("LD\"{0}\"{1}{2} ",
                                    carrierSelector.SelectedItem, Constants.DayNames[dayIndex], leaveSelector.SelectedItem));
                        }
                    });

                rowIndex += 1;

                while (rowIndex < maxRows)
                {
                    AddCell(MainGrid, "", x, rowIndex, null);
                    rowIndex += 1;
                }

            }

            UpdateMouseHilite();
            this.InvalidateVisual();
        }

        private void UpdateMouseHilite()
        {
            var pos = Mouse.GetPosition(MainGrid);
            var mouseRow = (int)(pos.Y / 20.0f);

            if (pos.Y < 0) mouseRow = -1;

            if (MouseHiliteRow >= 0)
                foreach (var child in MainGrid.Children)
                {
                    var textBlock = child as TextBlock;
                    if (textBlock != null)
                    {
                        var row = textBlock.GetValue(Grid.RowProperty) as int?;
                        var col = textBlock.GetValue(Grid.ColumnProperty) as int?;
                        if (col.HasValue && col.Value == 0) continue;
                        if (row.HasValue) textBlock.Background = (row.Value % 2 == 0) ? EvenRowBackground : OddRowBackground;
                    }
                }

            MouseHiliteRow = mouseRow;
            foreach (var child in MainGrid.Children)
            {
                var textBlock = child as TextBlock;
                if (textBlock != null)
                {
                    var row = textBlock.GetValue(Grid.RowProperty) as int?;
                    var col = textBlock.GetValue(Grid.ColumnProperty) as int?;
                    if (col.HasValue && col.Value == 0) continue;
                    if (row.HasValue && row.Value == mouseRow) textBlock.Background = MouseRowBackground;
                }
            }
        }


        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            UpdateMouseHilite();
            this.InvalidateVisual();
        }
    }
}
