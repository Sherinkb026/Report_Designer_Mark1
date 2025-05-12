using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using DevExpress.XtraReports.UI;

namespace Report_Mark1
{
    public partial class EditBox : UserControl
    {
        private Border chartBorderTag;

        public event EventHandler CloseRequested;

        public EditBox()
        {
            InitializeComponent();
        }

        public void LoadData(DataTable data, XRChart chart)
        {
            DataSourceComboBox.Items.Clear();
            DataSourceComboBox.Items.Add("Current Report Data");
            DataSourceComboBox.SelectedIndex = 0;

            XAxisComboBox.Items.Clear();
            YAxisComboBox.Items.Clear();
            foreach (DataColumn column in data.Columns)
            {
                XAxisComboBox.Items.Add(column.ColumnName);
                YAxisComboBox.Items.Add(column.ColumnName);
            }
            XAxisComboBox.SelectedIndex = 0;
            YAxisComboBox.SelectedIndex = 0;

            Tag = chart; // Store XRChart reference
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            if (Tag is XRChart chart && chartBorderTag is Border chartBorder)
            {
                // Example: Update chart placeholder (extend for actual chart config)
                var textBlock = chartBorder.Child as TextBlock;
                if (textBlock != null)
                {
                    textBlock.Text = $"Chart: {ChartTypeComboBox.SelectedItem?.ToString()} - {ChartTitleTextBox.Text}";
                }
                // TODO: Configure XRChart properties (e.g., series, data source)
            }
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}