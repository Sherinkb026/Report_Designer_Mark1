using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Report_Mark1
{
    /// <summary>
    /// Interaction logic for ReportTemplate.xaml
    /// </summary>
    public partial class ReportTemplate : UserControl
    {
        public ReportTemplate()
        {
            InitializeComponent();
        }

        // Method to load and bind the report data
        public void LoadReportData(IEnumerable<DataRow> fetchedData)
        {
            // Assuming you have a ViewModel that takes care of binding
            var reportViewModel = new ReportViewModel
            {
                ReportItems = fetchedData.Select(row => new ReportItem
                {
                    // Replace with the actual mapping logic for your data structure
                    Column1 = row["Bill No"].ToString(),
                    Column2 = row["Date"].ToString(),
                    Column3 = row["Total"].ToString()
                }).ToList(),
                CurrentDate = DateTime.Now.ToString("dd-MM-yyyy")
            };

            // Bind data context
            this.DataContext = reportViewModel;

            // Add custom template logic here if necessary
        }
    }

    // Define the ReportItem class
    public class ReportItem
    {
        public string Column1 { get; set; }  // Bill No
        public string Column2 { get; set; }  // Date
        public string Column3 { get; set; }  // Total
    }

    // Define the ReportViewModel class
    public class ReportViewModel
    {
        public List<ReportItem> ReportItems { get; set; }  // List of report items
        public string CurrentDate { get; set; }  // Current date in string format
    }
}
