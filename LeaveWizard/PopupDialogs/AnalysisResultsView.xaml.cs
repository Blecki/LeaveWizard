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

namespace LeaveWizard.PopupDialogs
{
    public partial class AnalysisResultsView : Window
    {
        private LeaveChart Chart;

        public AnalysisResultsView()
        {
            InitializeComponent();

            foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(LeaveAnalysis)))
                {
                    ReportSelectorComboBox.Items.Add(Activator.CreateInstance(type));
                }
            }

            ReportSelectorComboBox.SelectedIndex = 0;
        }

        public static void Show(LeaveChart Chart)
        {
            var popup = new AnalysisResultsView();
            popup.Chart = Chart;
            popup.ShowDialog();
        }

        private void PopulateData(LeaveAnalysisResults AnalysisResults)
        {
            DataGrid.RowDefinitions.Clear();
            DataGrid.ColumnDefinitions.Clear();
            DataGrid.Children.Clear();

            DataGrid.RowDefinitions.Add(new RowDefinition());
            DataGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (var c = 0; c < AnalysisResults.ColumnNames.Count; ++c)
            {
                DataGrid.ColumnDefinitions.Add(new ColumnDefinition());
                var block = AddValue(AnalysisResults.ColumnNames[c], 0, c);
                block.LayoutTransform = new RotateTransform(90);
            }

            foreach (var line in AnalysisResults.Rows)
            {
                DataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength() });
                AddValue(line.Name, DataGrid.RowDefinitions.Count - 1, 0);

                foreach (var column in line.Columns)
                {
                    var columnIndex = AnalysisResults.ColumnNames.IndexOf(column.Key);
                    if (columnIndex < 0) continue;
                    AddValue(column.Value.ToString(), DataGrid.RowDefinitions.Count - 1,
                        columnIndex + 1);
                }
            }

            UpdateLayout();
        }

        private TextBlock AddValue(String Value, int Row, int Column)
        {
            var block = new TextBlock
            {
                Text = Value,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(2,2,2,2)
            };

            Grid.SetColumn(block, Column);
            Grid.SetRow(block, Row);

            DataGrid.Children.Add(block);
            return block;
        }

        private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedReport = ReportSelectorComboBox.SelectedItem as LeaveAnalysis;
            PopulateData(selectedReport.Analyze(Chart));
        }

        private void ReportSelectorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedReport = ReportSelectorComboBox.SelectedItem as LeaveAnalysis;
            AnalysisProperties.SelectedObject = selectedReport;
        }
    }
}
