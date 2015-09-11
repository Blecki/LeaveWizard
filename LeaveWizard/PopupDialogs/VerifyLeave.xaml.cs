using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LeaveWizard
{
    /// <summary>
    /// Interaction logic for VerifyLeave.xaml
    /// </summary>
    public partial class VerifyLeave : Window
    {
        public LeaveChart Chart;
        Brush DefaultBackground;
        Brush ErrorBackground = new SolidColorBrush(Colors.Red);

        private DateTime StartDate;
        private DateTime EndDate;
        private String CarrierName;
        private bool StartDateValid = false;
        private bool EndDateValid = false;
        private bool CarrierNameValid = false;

        enum DayAction
        {
            Grant,
            Deny,
            Skip
        }

        private class AnalyzedDay
        {
            public DateTime Date;
            public int Week;
            public int Day;
            public DayAction Action;
            public String DenyReason;
        }

        private List<AnalyzedDay> AnalyzedDays;

        public VerifyLeave()
        {
            InitializeComponent();
            DefaultBackground = NameInput.Background;

            SetValid(NameInput, false);
            SetValid(StartInput, false);
            SetValid(EndInput, false);
            AnalyzeButton.IsEnabled = false;

            ApproveButton.IsEnabled = false;
            DenyButton.IsEnabled = false;

            foreach (var leaveType in Constants.AllLeaveTypes)
                LeaveSelector.Items.Add(leaveType);

            LeaveSelector.SelectedIndex = 0;
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            AnalysisGrid.Children.Clear();
            AnalysisGrid.RowDefinitions.Clear();
            ApproveButton.IsEnabled = false;
            DenyButton.IsEnabled = false;

            if ((EndDate - StartDate).Days > 60)
            {
                var promptResult = MessageBox.Show("The date range supplied is more than 60 days. Proceed?", "Whoa!", MessageBoxButton.YesNo);
                if (promptResult == MessageBoxResult.No) return;
            }

            var currentWeekIndex = FindStartWeekIndex();

            if (currentWeekIndex < 0)
            {
                AnalysisGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
                var message = new TextBlock
                {
                    Text = "The start date is before the first week in the leave chart.",
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                AnalysisGrid.Children.Add(message);
                return;
            }

            var leaveAddedThisWeek = 0;
            var currentDate = StartDate;
            AnalyzedDays = new List<AnalyzedDay>();

            while (currentDate <= EndDate)
            {
                int dayIndex = 0;
                #region Find day index.
                var dayDelta = currentDate - Chart.Weeks[currentWeekIndex].SaturdayDate;
                if (dayDelta.Days >= 7)
                {
                    var previousWeek = Chart.Weeks[currentWeekIndex];
                    currentWeekIndex += 1;
                    if (currentWeekIndex >= Chart.Weeks.Count) Chart.Weeks.Add(new WeekData());
                    Chart.Weeks[currentWeekIndex].Propogate(previousWeek);
                    leaveAddedThisWeek = 0; // New week.
                }
                else
                    dayIndex = dayDelta.Days;
                #endregion

                var analyzedDay = AnalyzeDay(currentWeekIndex, dayIndex, leaveAddedThisWeek);
                if (analyzedDay.Action == DayAction.Grant)
                    leaveAddedThisWeek += 1;

                analyzedDay.Date = currentDate;
                if (String.IsNullOrEmpty(analyzedDay.DenyReason)) analyzedDay.DenyReason = "Okay";

                AnalyzedDays.Add(analyzedDay);

                currentDate += TimeSpan.FromDays(1);
            }

            #region Update table
            foreach (var day in AnalyzedDays)
            {
                AnalysisGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });

                var dateText = new TextBlock
                {
                    Text = day.Date.DayOfWeek.ToString() + " " + day.Date.ToShortDateString(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                Grid.SetRow(dateText, AnalysisGrid.RowDefinitions.Count - 1);
                AnalysisGrid.Children.Add(dateText);

                var message = new TextBlock
                {
                    Text = day.DenyReason,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                if (day.Action == DayAction.Deny) message.Background = new SolidColorBrush(Colors.LightPink);
                Grid.SetRow(message, AnalysisGrid.RowDefinitions.Count - 1);
                Grid.SetColumn(message, 1);
                AnalysisGrid.Children.Add(message);
            }
            #endregion

            ApproveButton.IsEnabled = true;
            DenyButton.IsEnabled = true;
        }

        private int FindStartWeekIndex()
        {
            var activeWeek = 0;
            var currentWeekIndex = 0;
            while (true)
            {
                if (activeWeek >= Chart.Weeks.Count)
                    Chart.Weeks.Add(new WeekData()); // Expand the chart to support this leave.
                WeekData previousWeek = (activeWeek == 0) ? null : Chart.Weeks[activeWeek - 1];
                Chart.Weeks[activeWeek].Propogate(previousWeek);
                if (Chart.Weeks[activeWeek].SaturdayDate > StartDate)
                {
                    currentWeekIndex = activeWeek - 1;
                    break;
                }
                activeWeek += 1;
            }
            return currentWeekIndex;
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnalyzedDays == null || AnalyzedDays.Count == 0) return;

            var currentWeek = AnalyzedDays[0].Week;
            var commandQueue = "";

            foreach (var day in AnalyzedDays.Where(d => d.Action != DayAction.Skip))
            {
                if (day.Week != currentWeek)
                {
                    ApplyCommand(currentWeek, commandQueue);
                    currentWeek = day.Week;
                    commandQueue = "";
                }

                commandQueue += String.Format("L\"{0}\"{1}{2}\n", CarrierName, Constants.DayNames[day.Day], LeaveSelector.Text);
            }

            ApplyCommand(currentWeek, commandQueue);

            this.Close();
        }

        private void ApplyCommand(int Week, String Command)
        {
            if (String.IsNullOrEmpty(Command)) return;
            var previousWeek = Week == 0 ? null : Chart.Weeks[Week - 1];
            Chart.Weeks[Week].ApplyChange(previousWeek, Command);
        }

        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {
            if (AnalyzedDays == null || AnalyzedDays.Count == 0) return;

            var currentWeek = AnalyzedDays[0].Week;
            var commandQueue = "";

            foreach (var day in AnalyzedDays)
            {
                if (day.Week != currentWeek)
                {
                    ApplyCommand(currentWeek, commandQueue);
                    currentWeek = day.Week;
                    commandQueue = "";
                }

                commandQueue += String.Format("LD\"{0}\"{1}{2}\n", CarrierName, Constants.DayNames[day.Day], LeaveSelector.Text);
            }

            ApplyCommand(currentWeek, commandQueue);

            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CarrierName = NameInput.Text;
            CarrierNameValid = !String.IsNullOrEmpty(CarrierName);
            SetValid(NameInput, CarrierNameValid);
            AnalyzeButton.IsEnabled = CarrierNameValid && StartDateValid && EndDateValid;
        }

        private void StartInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            VerifyDate(StartInput.Text, out StartDate, out StartDateValid);
            SetValid(StartInput, StartDateValid);
            AnalyzeButton.IsEnabled = CarrierNameValid && StartDateValid && EndDateValid;
        }

        private void EndInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            VerifyDate(EndInput.Text, out EndDate, out EndDateValid);
            EndDateValid = EndDateValid & EndDate >= StartDate;
            SetValid(EndInput, EndDateValid);
            AnalyzeButton.IsEnabled = CarrierNameValid && StartDateValid && EndDateValid;
        }

        private void SetValid(TextBox Box, bool Valid)
        {
            if (Valid) Box.Background = DefaultBackground;
            else Box.Background = ErrorBackground;
        }

        private static void VerifyDate(String Text, out DateTime Date, out bool Valid)
        {
            try
            {
                Date = Convert.ToDateTime(Text);
                Valid = true;
            }
            catch (Exception x)
            {
                Date = DateTime.FromOADate(0);
                Valid = false;
            }
        }

        private AnalyzedDay AnalyzeDay(int WeekIndex, int DayIndex, int AdditionalLeaveThisWeek)
        {
            var result = new AnalyzedDay();
            result.Action = DayAction.Grant;

            result.Week = WeekIndex;
            result.Day = DayIndex;

            var isAmazonOnlyDay = DayIndex == (int)DayOfWeek.Sunday || Chart.Weeks[WeekIndex].DailySchedules[DayIndex].IsHoliday;
            var carrierIsSub = Chart.Weeks[WeekIndex].Substitutes.Find(s => s.Name == CarrierName) != null;

            //Check for existing leave.
            var existingLeave = Chart.Weeks[WeekIndex].DailySchedules[DayIndex].ReliefDays.Find(rd => rd.Carrier == CarrierName);
            if (existingLeave != null)
            {
                result.Action = DayAction.Skip;
                result.DenyReason = String.Format("Carrier already off. Leave type: {0}", existingLeave.LeaveType);
            }
            else //The carrier does not already have leave.
            {
                //Check number of available substitutes.
                if (Chart.Weeks[WeekIndex].Substitutes.Count <= Chart.Weeks[WeekIndex].DailySchedules[DayIndex].ReliefDays.Count)
                {
                    result.Action = DayAction.Deny;
                    result.DenyReason = "No coverage - Day already full.";
                }
                else //There are enough substitutes.
                {
                    if (isAmazonOnlyDay)
                    {
                        if (carrierIsSub) // For subs, check to be sure we have at least as many subs as Amazon routes.
                        {
                            var subsOffCount = Chart.Weeks[WeekIndex].DailySchedules[DayIndex].ReliefDays.Count(rd => Chart.Weeks[WeekIndex].Substitutes.Find(s => s.Name == rd.Carrier) != null);

                            if (subsOffCount + Chart.Weeks[WeekIndex].LeavePolicy.AmazonRoutes >= Chart.Weeks[WeekIndex].Substitutes.Count)
                            {
                                result.Action = DayAction.Deny;
                                result.DenyReason = "No coverage for Sunday or Holiday Amazon.";
                            }
                        }
                        else // Regulars already get sundays and holidays off.
                        {
                            result.Action = DayAction.Skip;
                            result.DenyReason = DayIndex == 1 ? "Sunday" : "Holiday";
                        }
                    }
                }
            }

            // Check if we approved too many days this week, unless it's an amazon-only day.
            if (result.Action == DayAction.Grant && !isAmazonOnlyDay && (Chart.Weeks[WeekIndex].SumLeaveDays() + AdditionalLeaveThisWeek) >= (Chart.Weeks[WeekIndex].LeavePolicy.DaysPerSub * Chart.Weeks[WeekIndex].Substitutes.Count))
            {
                if (carrierIsSub)
                {
                    var existingSchedule = Chart.Weeks[WeekIndex].DailySchedules[DayIndex].ReliefDays.Find(rd => rd.Substitute == CarrierName);
                    if (existingSchedule == null) // The week is full, but this sub isn't scheduled.
                    {
                        result.DenyReason = "Double-check schedule before granting this substitute leave.";
                        result.Action = DayAction.Deny;
                    }
                    else
                    {
                        result.DenyReason = "Substitute is scheduled. Double-check schedule before granting leave.";
                        result.Action = DayAction.Deny;
                    }
                }
                else
                {
                    result.Action = DayAction.Deny;
                    result.DenyReason = "Substitutes already working maximum days per leave policy.";
                }
            }

            // Check is someone else has already been denied this day.
            if (Chart.Weeks[WeekIndex].DailySchedules[DayIndex].DeniedLeave.Count != 0)
            {
                result.Action = DayAction.Deny;
                result.DenyReason = "Leave has already been denied another carrier on this day.";
            }

            return result;
        }
    }
}
