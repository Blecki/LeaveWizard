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
    public partial class ModifyBaseData : Window
    {
        Brush DefaultBackground;
        Brush ErrorBackground = new SolidColorBrush(Colors.Red);

        public DateTime SaturdayDate;
        public int Year;
        public int PayPeriod;
        public int Week;

        private bool ValidDate = false;
        private bool ValidYear = false;
        private bool ValidPayPeriod = false;
        private bool ValidWeek = false;

        public bool FinishedInput = false;

        public ModifyBaseData()
        {
            InitializeComponent();
            DefaultBackground = DateInput.Background;

            UpdateValidity();
            DateInput.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishedInput = true;
            this.Close();
        }

        private void DateInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                SaturdayDate = Convert.ToDateTime(DateInput.Text);
                ValidDate = true;
            }
            catch (Exception x)
            {
                ValidDate = false;
            }

            UpdateValidity();
        }

        private void SetColors(TextBox Element, bool Valid)
        {
            if (Valid) Element.Background = DefaultBackground;
            else Element.Background = ErrorBackground;
        }

        private void UpdateValidity()
        {
            SetColors(DateInput, ValidDate);
            SetColors(YearInput, ValidYear);
            SetColors(PayPeriodInput, ValidPayPeriod);
            SetColors(WeekInput, ValidWeek);

            Okay.IsEnabled = ValidDate && ValidYear && ValidPayPeriod && ValidWeek;
        }

        public static ModifyBaseData Show(WeekData Week)
        {
            var dialog = new ModifyBaseData();
            dialog.DateInput.Text = Week.SaturdayDate.ToShortDateString();
            dialog.YearInput.Text = Week.Year.ToString();
            dialog.PayPeriodInput.Text = Week.PayPeriod.ToString();
            dialog.WeekInput.Text = Week.Week.ToString();
            dialog.ShowDialog();
            return dialog;
        }

        private void ValidateNumberInput(out bool Valid, out int Value, String Text)
        {
            Valid = false;
            Value = 0;

            try
            {
                Value = Convert.ToInt32(Text);
                Valid = true;
            }
            catch (Exception e)
            {
                Valid = false;
            }

            UpdateValidity();
        }

        private void YearInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateNumberInput(out ValidYear, out Year, YearInput.Text);
        }

        private void PayPeriodInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateNumberInput(out ValidPayPeriod, out PayPeriod, PayPeriodInput.Text);
        }

        private void WeekInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateNumberInput(out ValidWeek, out Week, WeekInput.Text);
            if (Week != 1 && Week != 2)
            {
                ValidWeek = false;
                UpdateValidity();
            }
        }
    }
}
