using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Report_Mark1
{
    public partial class ReportTemplate : UserControl
    {
        public DataTable SourceDataTable { get; private set; }

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

            // Enable DataGrid row and column resizing
            reportDataGrid.CanUserResizeRows = true;
            reportDataGrid.CanUserResizeColumns = true;
            reportDataGrid.HeadersVisibility = DataGridHeadersVisibility.All;

            // Prevent tableBorder from capturing DataGrid mouse events
            tableBorder.PreviewMouseLeftButtonDown += (s, e) =>
            {
                if (e.OriginalSource is DependencyObject source)
                {
                    if (IsDataGridElement(source))
                    {
                        e.Handled = false; // Let DataGrid handle the event
                        return;
                    }
                }
                HeaderBorder_PreviewMouseLeftButtonDown(s, e);
            };
        }

        // Helper method to check if the event source is part of the DataGrid
        private bool IsDataGridElement(DependencyObject source)
        {
            while (source != null && source != reportDataGrid)
            {
                if (source is DataGrid || source is DataGridColumnHeader || source is DataGridRow)
                    return true;
                source = VisualTreeHelper.GetParent(source);
            }
            return source == reportDataGrid;
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
            SourceDataTable = reportData;
        }

        public string CurrentDate => DateTime.Now.ToString("yyyy-MM-dd");

        private void HeaderBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(headerBorder);
            SelectElement(headerBorder);
            e.Handled = false;
        }

        private void TableBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(tableBorder);
            SelectElement(tableBorder);
            e.Handled = false;
        }

        private void FooterBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.SelectElement(footerBorder);
            SelectElement(footerBorder);
            e.Handled = false;
        }

        public void SelectElement(UIElement element)
        {
            // Deselect previously selected element
            if (selectedElementBorder != null)
            {
                selectedElementBorder.BorderBrush = Brushes.Transparent;
                selectedElementBorder.BorderThickness = new Thickness(0);
            }

            selectedElement = element;

            if (element is TextBlock tb && tb.Parent is Border borderFromText)
            {
                borderFromText.BorderBrush = Brushes.Blue;
                borderFromText.BorderThickness = new Thickness(2);
                selectedElementBorder = borderFromText;
            }
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
         

        private void ReportCell_Click(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;

            if (sender is TextBlock textBlock && textBlock.Parent is Border border)
            {
                if (selectedCellBorder != null)
                {
                    selectedCellBorder.BorderBrush = Brushes.Transparent;
                    selectedCellBorder.BorderThickness = new Thickness(0);
                }

                border.BorderBrush = Brushes.LightCyan;
                border.BorderThickness = new Thickness(2);
                selectedCellBorder = border;

                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.SelectElement(textBlock);
            }
        }

        private void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                var parentWindow = Window.GetWindow(this) as MainWindow;
                parentWindow?.SelectElement(border);
                SelectElement(border);
            }

            e.Handled = false;
        }

        private void MainGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check if the clicked element is NOT a Border or a TextBlock
            var clickedElement = e.OriginalSource as DependencyObject;
            var border = FindParent<Border>(clickedElement);
            var textBlock = FindParent<TextBlock>(clickedElement);

            if (border == null && textBlock == null)
            {
                // Deselect the previously selected element
                if (selectedElementBorder != null)
                {
                    selectedElementBorder.BorderBrush = Brushes.Transparent;
                    selectedElementBorder.BorderThickness = new Thickness(0);
                    selectedElementBorder = null;
                    selectedElement = null;
                }
            }
        }

        // Utility method to walk up the visual tree
        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

    }
}