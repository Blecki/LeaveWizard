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
        private Brush DefaultBackground;
        private Brush ErrorBackground = new SolidColorBrush(Colors.Red);

        public AnalysisResultsView()
        {
            InitializeComponent();
            DefaultBackground = ReportSelectorComboBox.Background;
        }

        public static void Show(LeaveChart Chart)
        {
            var popup = new AnalysisResultsView();
            popup.Chart = Chart;

            foreach (var type in System.Reflection.Assembly.GetExecutingAssembly().GetTypes())
            {
                if (type.IsSubclassOf(typeof(LeaveAnalysis)))
                {
                    popup.ReportSelectorComboBox.Items.Add(Activator.CreateInstance(type, Chart));
                }
            }

            popup.ReportSelectorComboBox.SelectedIndex = 0; popup.ShowDialog();
        }

        private void PopulateData(LeaveAnalysisResults AnalysisResults)
        {
            DataGrid.RowDefinitions.Clear();
            DataGrid.ColumnDefinitions.Clear();
            DataGrid.Children.Clear();

            DataGrid.RowDefinitions.Add(new RowDefinition());

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
                    AddValue(column.Value, DataGrid.RowDefinitions.Count - 1,
                        columnIndex);
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
            ReportSummaryText.Text = selectedReport.Summary;

            ReportPropertyGrid.RowDefinitions.Clear();
            ReportPropertyGrid.Children.Clear();
            foreach (var property in selectedReport.GetType().GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    var foundIgnoreAttribute = false;
                    foreach (var attribute in property.GetCustomAttributes(false))
                        if (attribute.GetType() == typeof(IgnorwAnalysisPropertyAttribute))
                            foundIgnoreAttribute = true;
                    if (foundIgnoreAttribute) continue;

                    AnalysisPropertyConverter converter = null;
                    foreach (var attribute in property.GetCustomAttributes(false))
                        if (attribute.GetType() == typeof(AnalysisPropertyConverterAttribute))
                            converter = (attribute as AnalysisPropertyConverterAttribute).MakeConverter();
                    if (converter == null) converter = new GenericConverter(property.PropertyType);

                    var lambdaProperty = property;
                    ReportPropertyGrid.RowDefinitions.Add(new RowDefinition());

                    var propLabel = new Label { Content = property.Name };
                    Grid.SetRow(propLabel, ReportPropertyGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(propLabel, 0);
                    ReportPropertyGrid.Children.Add(propLabel);

                    var propEditBox = new TextBox { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
                    Grid.SetRow(propEditBox, ReportPropertyGrid.RowDefinitions.Count - 1);
                    Grid.SetColumn(propEditBox, 1);
                    ReportPropertyGrid.Children.Add(propEditBox);

                    DefaultBackground = propEditBox.Background;

                    propEditBox.Text = converter.ConvertToString(property.GetValue(selectedReport, null));

                    propEditBox.TextChanged += (_sender, _args) =>
                    {
                        var value = propEditBox.Text;
                        try
                        {
                            var newValue = converter.ConvertFromString(value);
                            lambdaProperty.SetValue(selectedReport, newValue, null);
                            propEditBox.Background = DefaultBackground;
                        }
                        catch (Exception xp)
                        {
                            propEditBox.Background = ErrorBackground;
                        }
                    };
                }
            }
        }
    }
}
