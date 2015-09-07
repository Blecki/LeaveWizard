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
    /// Interaction logic for CreateRegular.xaml
    /// </summary>
    public partial class Settings : Window
    {
        Brush DefaultBackground;
        Brush ErrorBackground = new SolidColorBrush(Colors.Red);

        public bool FinishedInput = false;

        public int DaysPerSub;
        public int AmazonRoutes;

        public bool DaysPerSubValid;
        public bool AmazonRoutesValid;

        public Settings()
        {
            InitializeComponent();
            DefaultBackground = DaysPerSubInput.Background;
            DaysPerSubInput.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishedInput = true;
            this.Close();
        }

        private void CheckValidity(TextBox Box, out bool Valid, out int Value)
        {
            try
            {
                Value = Convert.ToInt32(Box.Text);
                Valid = true;
            }
            catch (Exception x)
            {
                Value = 0;
                Valid = false;
            }

            UpdateValidity();
        }

        private void UpdateValidity()
        {
            UpdateValidity(DaysPerSubValid, DaysPerSubInput);
            UpdateValidity(AmazonRoutesValid, AmazonRoutesInput);
            Okay.IsEnabled = DaysPerSubValid && AmazonRoutesValid;
        }

        private void UpdateValidity(bool Valid, TextBox Box)
        {
            if (Valid) Box.Background = DefaultBackground;
            else Box.Background = ErrorBackground;
        }

        public static Settings Show(WeekData Week)
        {
            var dialog = new Settings();
            dialog.DaysPerSubInput.Text = Week.LeavePolicy.DaysPerSub.ToString();
            dialog.AmazonRoutesInput.Text = Week.LeavePolicy.AmazonRoutes.ToString();
            dialog.ShowDialog();
            return dialog;
        }

        private void DaysPerSubInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckValidity(DaysPerSubInput, out DaysPerSubValid, out DaysPerSub);
        }

        private void AmazonRoutesInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckValidity(AmazonRoutesInput, out AmazonRoutesValid, out AmazonRoutes);
        }
    }
}
