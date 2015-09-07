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
    public partial class CreateRegular : Window
    {
        Brush routeInputBackground;

        public int Route;
        public String Name;
        public bool FinishedInput = false;

        private bool ValidRoute = false;
        private bool ValidName = false;

        public CreateRegular()
        {
            InitializeComponent();
            routeInputBackground = RouteInput.Background;

            UpdateValidity();
            NameInput.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishedInput = true;
            this.Close();
        }

        private void RouteInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                Route = Convert.ToInt32(RouteInput.Text);
                ValidRoute = true;
            }
            catch (Exception x)
            {
                ValidRoute = false;
            }

            UpdateValidity();
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
            if (ValidName) NameInput.Background = routeInputBackground;
            else NameInput.Background = new SolidColorBrush(Colors.Red);
            if (ValidRoute) RouteInput.Background = routeInputBackground;
            else RouteInput.Background = new SolidColorBrush(Colors.Red);
            Okay.IsEnabled = ValidName && ValidRoute;
        }

        public static CreateRegular Show()
        {
            var dialog = new CreateRegular();
            dialog.ShowDialog();
            return dialog;
        }
    }
}
