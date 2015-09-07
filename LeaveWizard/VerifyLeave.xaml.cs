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
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            AnalysisGrid.Children.Clear();
            AnalysisGrid.RowDefinitions.Clear();
            ApproveButton.IsEnabled = false;
            DenyButton.IsEnabled = false;

            var startDate = Convert.ToDateTime(StartInput.Text);
            var endDate = Convert.ToDateTime(EndInput.Text);

            if ((endDate - startDate).Days > 60)
            {
                var promptResult = MessageBox.Show("The date range supplied is more than 60 days. Proceed?", "Whoa!", MessageBoxButton.YesNo);
                if (promptResult == MessageBoxResult.No) return;
            }

            var day = TimeSpan.FromDays(1);

            var activeWeek = 0;
            var currentWeekIndex = 0;
            while (true)
            {
                if (activeWeek >= Chart.Weeks.Count)
                    Chart.Weeks.Add(new WeekData()); // Expand the chart to support this leave.
                WeekData previousWeek = (activeWeek == 0) ? null : Chart.Weeks[activeWeek - 1];
                Chart.Weeks[activeWeek].Propogate(previousWeek);
                if (Chart.Weeks[activeWeek].SaturdayDate > startDate)
                {
                    currentWeekIndex = activeWeek - 1;
                    break;
                }
                activeWeek += 1;
            }

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
            }

            var leaveAddedThisWeek = 0;

            while (startDate <= endDate)
            {
                var dayShouldBeDenied = false;
                var dayMessage = "";
                var done = false;

                AnalysisGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });

                // Display date in row.                
                var dateText = new TextBlock
                {
                    Text = startDate.DayOfWeek.ToString() + " " + startDate.ToShortDateString(),
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                Grid.SetRow(dateText, AnalysisGrid.RowDefinitions.Count - 1);
                AnalysisGrid.Children.Add(dateText);

                //Find day index.
                int dayIndex = 0;
                var dayDelta = startDate - Chart.Weeks[currentWeekIndex].SaturdayDate;
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

                //Analyze day.
                var existingLeave = Chart.Weeks[currentWeekIndex].DailySchedules[dayIndex].ReliefDays.Find(rd => rd.Carrier == CarrierName);
                if (existingLeave != null)
                {
                    done = true;
                    dayMessage = String.Format("Carrier already off. Leave type: {0}", existingLeave.LeaveType);
                }

                if (Chart.Weeks[currentWeekIndex].Substitutes.Count <= Chart.Weeks[currentWeekIndex].DailySchedules[dayIndex].ReliefDays.Count)
                {
                    done = true;
                    dayMessage = "Day full.";
                    dayShouldBeDenied = true;
                }

                if (dayIndex == 1 /* Sunday */ || Chart.Weeks[currentWeekIndex].DailySchedules[dayIndex].IsHoliday)
                {
                    var sub = Chart.Weeks[currentWeekIndex].Substitutes.Find(s => s.Name == CarrierName);
                    if (sub == null)
                    {
                        done = true;
                        dayMessage = dayIndex == 1 ? "Sunday" : "Holiday";
                    }
                    else
                    {
                        var subsOffCount = Chart.Weeks[currentWeekIndex].DailySchedules[dayIndex].ReliefDays.Count(rd => Chart.Weeks[currentWeekIndex].Substitutes.Find(s => s.Name == rd.Carrier) != null);
                        if (subsOffCount + Chart.Weeks[currentWeekIndex].LeavePolicy.AmazonRoutes >= Chart.Weeks[currentWeekIndex].Substitutes.Count)
                        {
                            done = true;
                            dayMessage = "Too many subs off already.";
                            dayShouldBeDenied = true;
                        }
                    }
                }

                if (!done && (Chart.Weeks[currentWeekIndex].SumLeaveDays() + leaveAddedThisWeek) >= (Chart.Weeks[currentWeekIndex].Substitutes.Count * Chart.Weeks[currentWeekIndex].LeavePolicy.DaysPerSub))
                {
                    done = true;
                    dayMessage = "Week full";
                    dayShouldBeDenied = true;
                }

                if (!dayShouldBeDenied) leaveAddedThisWeek += 1;
                if (!dayShouldBeDenied && String.IsNullOrEmpty(dayMessage)) dayMessage = "Okay";

                var message = new TextBlock
                {
                    Text = dayMessage,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = System.Windows.VerticalAlignment.Center
                };
                if (dayShouldBeDenied) message.Background = new SolidColorBrush(Colors.MediumVioletRed);
                Grid.SetRow(message, AnalysisGrid.RowDefinitions.Count - 1);
                Grid.SetColumn(message, 1);
                AnalysisGrid.Children.Add(message);

                startDate += day;
            }

            ApproveButton.IsEnabled = true;
            DenyButton.IsEnabled = true;
        }

        private void ApproveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void DenyButton_Click(object sender, RoutedEventArgs e)
        {

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

    }
}
