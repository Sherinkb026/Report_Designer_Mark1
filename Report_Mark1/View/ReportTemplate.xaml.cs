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



        //private void Element_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    DependencyObject clickedElement = e.OriginalSource as DependencyObject;

        //    var textBlock = FindParent<TextBlock>(clickedElement);
        //    var textBox = FindParent<TextBox>(clickedElement);
        //    var richTextBox = FindParent<RichTextBox>(clickedElement);
        //    var parentWindow = Window.GetWindow(this) as MainWindow;

        //    bool selectionMade = false;

        //    if (textBlock != null)
        //    {
        //        parentWindow?.SelectElement(textBlock);
        //        SelectElement(textBlock);
        //        selectionMade = true;
        //    }
        //    else if (textBox != null)
        //    {
        //        parentWindow?.SelectElement(textBox);
        //        SelectElement(textBox);
        //        selectionMade = true;
        //    }
        //    else if (richTextBox != null)
        //    {
        //        parentWindow?.SelectElement(richTextBox);
        //        SelectElement(richTextBox);
        //        selectionMade = true;
        //    }
        //    else if (sender is Border border)
        //    {
        //        parentWindow?.SelectElement(border);
        //        SelectElement(border);
        //        selectionMade = true;
        //    }

        //    e.Handled = selectionMade; // ✅ only stop bubbling if something was selected
        //}


        //private void MainGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    // Get the clicked element
        //    var clickedElement = e.OriginalSource as DependencyObject;
        //    var border = FindParent<Border>(clickedElement);
        //    var textBlock = FindParent<TextBlock>(clickedElement);

        //    // If the clicked element is not a Border or TextBlock, deselect the current element
        //    if (border == null && textBlock == null)
        //    {
        //        var parentWindow = Window.GetWindow(this) as MainWindow;
        //        parentWindow?.SelectElement(null); // Deselect in MainWindow
        //        SelectElement(null); // Deselect in ReportTemplate
        //        e.Handled = true; // Mark the event as handled
        //    }
        //}

        /// /////////////

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
        /// <summary>
        /// //////////




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



        // ✅ Helper to filter only those borders you marked selectable (not layout/outer borders)
        //private bool IsSelectableBorder(Border border)
        //{
        //    if (border == null) return false;

        //    // Check by name or some other logic if it's a selectable border
        //    return border.Name == "headerBorder" || border.Name == "tableBorder" || border.Name == "footerBorder";
        //}


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