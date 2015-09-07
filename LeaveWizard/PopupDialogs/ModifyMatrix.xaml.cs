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
    public partial class ModifyMatrix : Window
    {
        Brush DefaultBackground;
        Brush ErrorBackground = new SolidColorBrush(Colors.Red);

        public bool FinishedInput = false;

        public int[] Choices = new int[] { 0, 0, 0 };
        private bool[] Valid = new bool[] { false, false, false };

        public ModifyMatrix()
        {
            InitializeComponent();
            DefaultBackground = InputA.Background;

            InputA.Tag = 0;
            InputB.Tag = 1;
            InputC.Tag = 2;

            InputA.Focus();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FinishedInput = true;
            this.Close();
        }

        private void Input_TextChanged(object sender, TextChangedEventArgs e)
        {
            var box = sender as TextBox;
            if (box == null) return;

            var text = box.Text;
            var index = (box.Tag as int?).Value;

            try
            {
                Choices[index] = Convert.ToInt32(text);
                Valid[index] = true;
            }
            catch (Exception x)
            {
                Valid[index] = false;
            }

            UpdateValidity();
        }

        private void UpdateValidity()
        {
            UpdateValidity(Valid[0], InputA);
            UpdateValidity(Valid[1], InputB);
            UpdateValidity(Valid[2], InputC);
            Okay.IsEnabled = Valid[0] && Valid[1] && Valid[2];
        }

        private void UpdateValidity(bool Valid, TextBox Box)
        {
            if (Valid) Box.Background = DefaultBackground;
            else Box.Background = ErrorBackground;
        }

        public static ModifyMatrix Show(Substitute Sub)
        {
            var dialog = new ModifyMatrix();
            dialog.InputA.Text = Sub.Matrix[0].ToString();
            dialog.InputB.Text = Sub.Matrix[1].ToString();
            dialog.InputC.Text = Sub.Matrix[2].ToString();
            dialog.ShowDialog();
            return dialog;
        }
    }
}
