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
    /// Interaction logic for SimpleSelector.xaml
    /// </summary>
    public partial class SimpleSelector : Window
    {
        public Object SelectedItem = null;
        public bool SelectionMade = false;

        public SimpleSelector()
        {
            InitializeComponent();
        }

        public SimpleSelector(IEnumerable<Object> Options)
        {
            InitializeComponent();
            OptionBox.ItemsSource = Options;
        }

        private void OptionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.SelectedItem = OptionBox.SelectedItem;
            this.SelectionMade = true;
            this.Close();
        }

        public static SimpleSelector Show(String Message, IEnumerable<Object> Options)
        {
            var selector = new SimpleSelector(Options);
            selector.Title = Message;
            selector.ShowDialog();
            return selector;
        }
    }
}
