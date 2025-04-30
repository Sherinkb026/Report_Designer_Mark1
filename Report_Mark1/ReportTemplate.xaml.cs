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
        public DataTable SourceDataTable { get; private set; }

        // Variables for dragging
        private bool isDragging = false;
        private Point mouseOffset;
        private UIElement draggedElement = null;
        private UIElement selectedElement;
        private Border selectedCellBorder;
        private Border selectedElementBorder;



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



        #region Canvas

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

        // Reset the selected element's border to invisible when mouse is released
        public void SelectElement(UIElement element)
        {
            // Deselect previously selected element
            if (selectedElementBorder != null)
            {
                selectedElementBorder.BorderBrush = Brushes.Transparent;
                selectedElementBorder.BorderThickness = new Thickness(0);
            }

            selectedElement = element;

            // Case 1: TextBlock inside a Border
            if (element is TextBlock tb && tb.Parent is Border borderFromText)
            {
                borderFromText.BorderBrush = Brushes.Blue;
                borderFromText.BorderThickness = new Thickness(2);
                selectedElementBorder = borderFromText;
            }
            // Case 2: The element itself is a Border
            else if (element is Border border)
            {
                border.BorderBrush = Brushes.Blue;
                border.BorderThickness = new Thickness(2);
                selectedElementBorder = border;
            }
            else
            {
                selectedElementBorder = null;
            }
        }


        #endregion


        public class ReportViewModel
        {
            public DataTable ReportItems { get; set; }
            public string CurrentDate { get; set; }
        }


        private void ReportCell_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (sender is TextBlock textBlock && textBlock.Parent is Border border)
            {
                // Remove previous selection style
                if (selectedCellBorder != null)
                {
                    selectedCellBorder.BorderBrush = Brushes.Transparent;
                    selectedCellBorder.BorderThickness = new Thickness(0);
                }

                // Highlight current cell
                border.BorderBrush = Brushes.Blue;
                border.BorderThickness = new Thickness(2);
                selectedCellBorder = border;

                // Pass to MainWindow
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?                                                                                                                                                                                  .SelectElement(textBlock); // You are selecting the TextBlock, not Border
            }
        }

        private void FooterBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(footerBorder);
        }
    }
}

   