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
            //MainGrid.AddHandler(UIElement.PreviewMouseLeftButtonDownEvent,
            //new MouseButtonEventHandler(Element_PreviewMouseLeftButtonDown), true);


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
                Element_PreviewMouseLeftButtonDown(s, e);
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

            decimal grandTotal = 0;

            foreach (var row in dataRows)
            {
                string productName = row["Name"].ToString();
                string priceStr = row["Price"].ToString();

                int quantity = 1; // or whatever logic you want
                decimal price = decimal.TryParse(priceStr, out var p) ? p : 0;
                decimal total = price * quantity;

                reportData.Rows.Add(productName, quantity.ToString(), $"${price}", $"${total}");
                grandTotal += total;
            }

            reportData.Rows.Add("TOTAL", "", "", $"${grandTotal}");
            reportDataGrid.ItemsSource = reportData.DefaultView;
            SourceDataTable = reportData;
        }


        public string CurrentDate => DateTime.Now.ToString("yyyy-MM-dd");



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
            DependencyObject clickedElement = e.OriginalSource as DependencyObject;

            var parentWindow = Window.GetWindow(this) as MainWindow;

            // Traverse up to find the closest container (Border, Panel, TextBlock, etc.)
            var selectableElement = FindSelectableVisual(clickedElement);

            if (selectableElement is UIElement uiElement)
            {
                parentWindow?.SelectElement(uiElement);
                SelectElement(uiElement);
            }

            e.Handled = false;
        }
        private UIElement FindSelectableVisual(DependencyObject start)
        {
            while (start != null)
            {
                if (start is Border || start is TextBlock || start is TextBox || start is RichTextBox || start is StackPanel || start is Grid)
                {
                    return start as UIElement;
                }

                start = VisualTreeHelper.GetParent(start);
            }
            return null;
        }




        private void MainGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = e.OriginalSource as DependencyObject;

            // Let WPF handle resizing if the user is on a resize grip
            if (FindParent<System.Windows.Controls.Primitives.Thumb>(clickedElement) != null)
            {
                return; // Don't mark event as handled
            }

            var border = FindParent<Border>(clickedElement);
            var textBlock = FindParent<TextBlock>(clickedElement);
            var textBox = FindParent<TextBox>(clickedElement);
            var richTextBox = FindParent<RichTextBox>(clickedElement);

            bool isInSelectableBorder = IsInsideSelectableBorder(clickedElement);

            if (textBlock == null && textBox == null && richTextBox == null && !isInSelectableBorder)
            {
                var parentWindow = Window.GetWindow(this) as MainWindow;
                parentWindow?.SelectElement(null);
                SelectElement(null);
                // ✅ Removed e.Handled = true to let WPF handle resizing/etc.
            }
        }



        private bool IsInsideSelectableBorder(DependencyObject element)
        {
            while (element != null)
            {
                if (element is Border border &&
                    (border.Name == "headerBorder" || border.Name == "tableBorder" || border.Name == "footerBorder"))
                {
                    return true;
                }
                element = VisualTreeHelper.GetParent(element);
            }
            return false;
        }

        private bool IsInsideDataGridResizer(DependencyObject source)
        {
            while (source != null)
            {
                if (source is System.Windows.Controls.Primitives.Thumb thumb)
                {
                    return true;
                }
                source = VisualTreeHelper.GetParent(source);
            }
            return false;
        }






        public void SelectElement(UIElement element)
        {
            // Reset the previous selection's border
            if (selectedElementBorder != null)
            {
                selectedElementBorder.BorderBrush = Brushes.Gray;
                selectedElementBorder.BorderThickness = new Thickness(1);
            }

            // Clear the previous cell border if it exists
            if (selectedCellBorder != null)
            {
                selectedCellBorder.BorderBrush = Brushes.Gray;
                selectedCellBorder.BorderThickness = new Thickness(1);
            }

            selectedElement = element;
            selectedElementBorder = null;
            selectedCellBorder = null;

            if (element is TextBlock tb && tb.Parent is Border borderFromText)
            {
                borderFromText.BorderBrush = Brushes.Cyan;
                borderFromText.BorderThickness = new Thickness(1);
                selectedElementBorder = borderFromText;
            }
            else if (element is Border border)
            {
                border.BorderBrush = Brushes.Cyan;
                border.BorderThickness = new Thickness(1);
                selectedElementBorder = border;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.DeleteElement();
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



        private CommandManager _commandManager;

        public void SetCommandManager(CommandManager manager)
        {
            _commandManager = manager;
        }

    }
}