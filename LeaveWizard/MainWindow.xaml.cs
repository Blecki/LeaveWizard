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
using PdfSharp;
using PdfSharp.Drawing;

namespace LeaveWizard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        LeaveChart Data;
        WeekData CurrentWeek { get { return Data.Weeks[Data.CurrentWeek]; } }
        WeekData PreviousWeek { get { return Data.CurrentWeek == 0 ? null : Data.Weeks[Data.CurrentWeek - 1]; } }
        bool EmergencyClose = false;
                
        public MainWindow()
        {
            InitializeComponent();

            try
            {
                Data = new LeaveChart();
                Data.LoadData();
                ApplyChangeToCurrentWeek("");
                UpdateDisplay();
            }
            catch (Exception e)
            {
                EmergencyClose = true;
                this.Close();
            }

            WeekChart.ApplyAction += ApplyChangeToCurrentWeek;

            AdminPasswordInput_TextChanged(null, null);
        }

        private void UpdateData()
        {
            CurrentWeek.Propogate(PreviousWeek);

            if (Data.CurrentWeek < Data.Weeks.Count - 1 && Data.Weeks[Data.CurrentWeek + 1].DailySchedules[0].IsHoliday)
            {
                Data.Weeks[Data.CurrentWeek + 1].Propogate(CurrentWeek);
                Data.Weeks[Data.CurrentWeek + 1].Holiday(0);
            }
        }

        private void UpdateDisplay()
        {
            UpdateData();

            WeekInfoLabel.Content = String.Format("PayPeriod {0}, Week {1}", Data.Weeks[Data.CurrentWeek].PayPeriod, Data.Weeks[Data.CurrentWeek].Week);

            var numSubs = Data.Weeks[Data.CurrentWeek].Substitutes.Count;
            var sumLeaveDays = Data.Weeks[Data.CurrentWeek].SumLeaveDays();


            SubCountInfoLineLabel.Content = String.Format("Number of subs: {0}  Days per sub: {1} Amazon Routes: {2}  Leave days: {3}  Days still schedulable: {4}", numSubs, CurrentWeek.LeavePolicy.DaysPerSub, CurrentWeek.LeavePolicy.AmazonRoutes, sumLeaveDays, (numSubs * CurrentWeek.LeavePolicy.DaysPerSub) - (sumLeaveDays));

            WeekChart.DisplayWeek(Data.Weeks[Data.CurrentWeek]);
            this.InvalidateVisual();
        }

        private void PrevWeek_Click(object sender, RoutedEventArgs e)
        {
            if (Data.CurrentWeek == 0) System.Console.Beep();
            else
            {
                Data.CurrentWeek = Data.CurrentWeek - 1;
                UpdateDisplay();
            }
        }

        private void Now_Click(object sender, RoutedEventArgs e)
        {
            var now = DateTime.Now;
            var yearAhead = CurrentWeek.SaturdayDate + TimeSpan.FromDays(365);
            if (yearAhead < now)
            {
                var result = MessageBox.Show("The current week is more than one year in the past. There might be a problem with your data. If the dates on the current week are wrong, you should correct them before navigating to present day. If they are correct, you should consider rebasing to a more recent week. Do you want to try and navigate to the present day anyway?", "Data integrity issue detected!", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.No) return;
            }

            Data.CurrentWeek = 0;

            while (Data.Weeks[Data.CurrentWeek].SaturdayDate + TimeSpan.FromDays(7) < DateTime.Now)
            {
                if (Data.CurrentWeek == Data.Weeks.Count - 1) break;
                Data.CurrentWeek += 1;
                CurrentWeek.Propogate(PreviousWeek);
            }

            UpdateDisplay();
        }

        private void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            if (Data.CurrentWeek == Data.Weeks.Count - 1)
                Data.Weeks.Add(new WeekData());
            Data.CurrentWeek = Data.CurrentWeek + 1;
            UpdateDisplay();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!EmergencyClose) Data.SaveData();
        }

        private void GenSchedule_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void WeekInfoLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var changes = ModifyBaseData.Show(CurrentWeek);
            if (changes.FinishedInput)
                ApplyChangeToCurrentWeek(String.Format("B\"{0}\" {1} {2} {3} ", changes.SaturdayDate.ToShortDateString(), changes.Year, changes.PayPeriod, changes.Week));
        }

        private void Rebase_Click(object sender, RoutedEventArgs e)
        {
            var firstConfirmation = MessageBox.Show("This operation cannot be reversed. All data before the current week will be lost. Are you sure you want to do this?", "Confirm Dangerous Operation", MessageBoxButton.YesNo);
            if (firstConfirmation == MessageBoxResult.No) return;

            if (CurrentWeek.SaturdayDate > DateTime.Now)
                if (MessageBox.Show("The currently displayed week is in the future. Rebasing will erase all data before this future date. You will not be able to recover it. Are you sure you want to do this?", "Confirm Really Dangerous Operation", MessageBoxButton.YesNo) == MessageBoxResult.No) return;

            if (Data.CurrentWeek == 0) return; //Nothing to remove.

            var genCode = CodeGenerator.GenerateChangeCode(null, CurrentWeek);
            CurrentWeek.EffectiveChanges = genCode;

            Data.Weeks.RemoveRange(0, Data.CurrentWeek);
            Data.CurrentWeek = 0;

            UpdateDisplay();
        }

        private void ViewCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var codeWindow = CodeWindow.Show(Data.Weeks[Data.CurrentWeek].EffectiveChanges);
            Data.Weeks[Data.CurrentWeek].EffectiveChanges = codeWindow.Text;
            UpdateDisplay();
        }

        private void GenCodeButton_Click(object sender, RoutedEventArgs e)
        {
            var codeWindow = CodeWindow.Show(CodeGenerator.GenerateChangeCode(PreviousWeek, CurrentWeek));
        }

        private void ApplyChangeToCurrentWeek(String ChangeCommand)
        {
            CurrentWeek.ApplyChange(PreviousWeek, ChangeCommand);
            UpdateDisplay();
        }

        private void HelpMenuItem_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("help.html");
        }

        private void AdminPasswordInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            var correctPassword = AdminPasswordInput.Text == "Stafford";
            GenCodeButton.IsEnabled = correctPassword;
            ViewCodeButton.IsEnabled = correctPassword;
        }

        private void PrintSubScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            string filename = Guid.NewGuid().ToString().ToUpper() + ".pdf";
            var document = new PdfSharp.Pdf.PdfDocument(filename);
            document.Info.Creator = "Schedule";
            var page = document.AddPage();
            page.Size = PageSize.A4;
            page.Orientation = PageOrientation.Landscape;
            var gfx = XGraphics.FromPdfPage(page);
            LeaveChart.DrawSubSchedule(gfx, CurrentWeek);
            document.Close();
            System.Diagnostics.Process.Start(filename);
        }

        private void SubCountInfoLineLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var settings = Settings.Show(CurrentWeek);
            if (settings.FinishedInput)
                ApplyChangeToCurrentWeek(String.Format("P{0} {1} ", settings.DaysPerSub, settings.AmazonRoutes));
        }

        private void LeaveWizardButton_Click(object sender, RoutedEventArgs e)
        {
            var wizard = new VerifyLeave();
            wizard.Chart = Data;
            wizard.ShowDialog();

            UpdateDisplay();
        }

    }
}
