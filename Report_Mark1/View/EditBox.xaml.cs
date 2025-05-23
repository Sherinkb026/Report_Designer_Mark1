using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using DevExpress.XtraReports.UI;
using DevExpress.XtraCharts;

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

        public void SetChartBorder(Border border)
        {
            chartBorderTag = border;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            XRChart chart = Tag as XRChart;
            Border chartBorder = chartBorderTag as Border;

            if (chart != null && chartBorder != null)
            {
                if (XAxisComboBox.SelectedItem == null || YAxisComboBox.SelectedItem == null)
                    return;

                string xField = XAxisComboBox.SelectedItem.ToString();
                string yField = YAxisComboBox.SelectedItem.ToString();
                string chartTitle = ChartTitleTextBox.Text;
                string chartType = ChartTypeComboBox.SelectedItem != null ? ChartTypeComboBox.SelectedItem.ToString() : "";

                // Set chart view type
                ViewType viewType;
                switch (chartType)
                {
                    case "Bar":
                        viewType = ViewType.Bar;
                        break;
                    case "Line":
                        viewType = ViewType.Line;
                        break;
                    case "Pie":
                        viewType = ViewType.Pie;
                        break;
                    default:
                        viewType = ViewType.Bar;
                        break;
                }

                // Configure the chart
                chart.Series.Clear();

                Series series = new Series(chartTitle, viewType);
                series.ArgumentDataMember = xField;
                series.ValueDataMembers.AddRange(yField);
                chart.Series.Add(series);

                // Set data source
                if (!(chart.DataSource is DataTable))
                {
                    chart.DataSource = (chartBorder.Tag as DataTable) ?? new DataTable();
                }

                chart.Titles.Clear();
                chart.Titles.Add(new ChartTitle() { Text = chartTitle });

                // Update placeholder
                if (chartBorder.Child is TextBlock textBlock)
                {
                    textBlock.Text = $"Chart: {chartType} - {chartTitle}";
                }
            }

            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public void ForceClose()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
            this.Visibility = Visibility.Collapsed;
            this.Tag = null;
            this.chartBorderTag = null;
        }

    }
}
