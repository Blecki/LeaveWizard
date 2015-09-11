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
        public AnalysisResultsView()
        {
            InitializeComponent();
        }

        public static void Show(LeaveAnalysisResults AnalysisResults)
        {
            var popup = new AnalysisResultsView();
            var columnHeaders = new List<String>();

            foreach (var line in AnalysisResults)
            {
                popup.DataGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength() });
                AddValue(line.Carrier, popup.DataGrid.RowDefinitions.Count - 1, 0, popup.DataGrid);
                
                foreach (var leaveType in line.LeaveUsage)
                {
                    var columnIndex = columnHeaders.IndexOf(leaveType.Key);
                    if (columnIndex < 0)
                    {
                        columnIndex = columnHeaders.Count;
                        columnHeaders.Add(leaveType.Key);
                    }

                    AddValue(leaveType.Value.ToString(), popup.DataGrid.RowDefinitions.Count - 1,
                        columnIndex + 1, popup.DataGrid);
                }
            }

            for (var columnIndex = 0; columnIndex < columnHeaders.Count; ++columnIndex)
            {
                popup.DataGrid.ColumnDefinitions.Add(new ColumnDefinition());
                var block = AddValue(columnHeaders[columnIndex], 0, columnIndex + 1, popup.DataGrid);
                block.LayoutTransform = new RotateTransform(90);
            }

            popup.UpdateLayout();

            popup.ShowDialog();
        }

        private static TextBlock AddValue(String Value, int Row, int Column, Grid To)
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

            To.Children.Add(block);
            return block;
        }
    }
}
