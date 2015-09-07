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
    public partial class CreateSub : Window
    {
        Brush DefaultBackground;

        public String Name;
        public bool FinishedInput = false;

        private bool ValidName = false;

        public CreateSub()
        {
            InitializeComponent();
            DefaultBackground = NameInput.Background;

            UpdateValidity();
            NameInput.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishedInput = true;
            this.Close();
        }

        private void NameInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(NameInput.Text))
            {
                Name = NameInput.Text;
                ValidName = true;
            }
            else
                ValidName = false;

            UpdateValidity();
        }

        private void UpdateValidity()
        {
            if (ValidName) NameInput.Background = DefaultBackground;
            else NameInput.Background = new SolidColorBrush(Colors.Red);
            Okay.IsEnabled = ValidName;
        }

        public static CreateSub Show()
        {
            var dialog = new CreateSub();
            dialog.ShowDialog();
            return dialog;
        }
    }
}
