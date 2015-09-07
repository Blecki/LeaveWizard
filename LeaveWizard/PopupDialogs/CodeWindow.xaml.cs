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
    public partial class CodeWindow : Window
    {
        public String Text { get { return CodeInput.Text; } set { CodeInput.Text = value; } }

        public CodeWindow()
        {
            InitializeComponent();
        }

        public static CodeWindow Show(String Data)
        {
            var window = new CodeWindow();
            window.Text = Data;
            window.ShowDialog();
            return window;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
