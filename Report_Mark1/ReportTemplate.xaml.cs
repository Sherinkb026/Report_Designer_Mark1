using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Report_Mark1
{
    public partial class ReportTemplate : UserControl
    {
        public ReportTemplate()
        {
            InitializeComponent();
            DataContext = this;

            if (this.FindName("headerBorder") == null ||
                this.FindName("tableBorder") == null ||
                this.FindName("footerBorder") == null)
            {
                throw new InvalidOperationException("One or more border elements (headerBorder, tableBorder, footerBorder) were not found in the XAML.");
            }
        }

        public void LoadReportData(IEnumerable<DataRow> dataRows)
        {
            DataTable reportData = new DataTable();
            reportData.Columns.Add("Description", typeof(string));
            reportData.Columns.Add("Quantity", typeof(string));
            reportData.Columns.Add("Price", typeof(string));
            reportData.Columns.Add("Total", typeof(string));

            int index = 0;
            foreach (var row in dataRows)
            {
                if (index >= 5) break;
                string description = $"Description of item or service goes here.";
                string quantity = (index % 2 == 0) ? "5" : "1";
                string price = (index % 2 == 0) ? "$100" : "$150";
                string total = (index % 2 == 0) ? "$500" : "$150";
                reportData.Rows.Add(description, quantity, price, total);
                index++;
            }

            reportData.Rows.Add("TOTAL", "", "", "$5000");
            reportDataGrid.ItemsSource = reportData.DefaultView;
        }

        public string CurrentDate => DateTime.Now.ToString("yyyy-MM-dd");

        private void HeaderBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(headerBorder);
        }

        private void TableBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(tableBorder);
        }

        private void FooterBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(footerBorder);
        }
    }
}