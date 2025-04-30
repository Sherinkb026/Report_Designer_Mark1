using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private Border selectedElement = null;
        private Border selectedCellBorder;


        public ReportTemplate()
        {
            InitializeComponent();

            // Attach drag handlers to each border
            headerBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            headerBorder.PreviewMouseMove += Element_MouseMove;
            headerBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            tableBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            tableBorder.PreviewMouseMove += Element_MouseMove;
            tableBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            footerBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            footerBorder.PreviewMouseMove += Element_MouseMove;
            footerBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            // Set initial positions
            Canvas.SetLeft(headerCanvas, 0);
            Canvas.SetTop(headerCanvas, 0);

            Canvas.SetLeft(tableCanvas, 0);
            Canvas.SetTop(tableCanvas, 120); // Adjusted to give more space below header

            Canvas.SetLeft(footerCanvas, 0); // Full width, aligned left
            Canvas.SetTop(footerCanvas, 650); // Positioned further down to fit content
        }

        public void LoadReportData(IEnumerable<DataRow> fetchedData)
        {
            if (fetchedData == null || !fetchedData.Any())
            {
                MessageBox.Show("No data to display in the report.",
                    "Empty Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Store the original DataTable reference
            SourceDataTable = fetchedData.First().Table;

            // Create the ViewModel
            var reportViewModel = new ReportViewModel
            {
                ReportItems = SourceDataTable,
                CurrentDate = DateTime.Now.ToString("dd-MM-yyyy")
            };

            // Bind data context
            this.DataContext = reportViewModel;
        }

        #region Dragging Handlers

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Border border = sender as Border;
            if (border != null)
            {
                // Select the element
                if (selectedElement != null)
                {
                    selectedElement.BorderBrush = Brushes.Transparent; // Reset to transparent
                    selectedElement.BorderThickness = new Thickness(0); // Reset to 0 thickness
                }
                selectedElement = border;
                selectedElement.BorderBrush = Brushes.Blue;
                selectedElement.BorderThickness = new Thickness(2);

                // Only start dragging if the mouse is held down for a drag action
                if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed)
                {
                    draggedElement = sender as UIElement;
                    mouseOffset = e.GetPosition(draggedElement);
                    isDragging = true;
                    draggedElement.CaptureMouse();
                }
                else
                {
                    // Allow single clicks to pass through for editing (e.g., DataGrid cells)
                    e.Handled = false;
                }
            }
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                Canvas parentCanvas = null;
                if (draggedElement == headerBorder)
                    parentCanvas = headerCanvas;
                else if (draggedElement == tableBorder)
                    parentCanvas = tableCanvas;
                else if (draggedElement == footerBorder)
                    parentCanvas = footerCanvas;

                if (parentCanvas != null)
                {
                    Point position = e.GetPosition(mainCanvas);
                    Canvas.SetLeft(parentCanvas, position.X - mouseOffset.X);
                    Canvas.SetTop(parentCanvas, position.Y - mouseOffset.Y);
                }
            }
            else
            {
                // Allow mouse move events to pass through if not dragging
                e.Handled = false;
            }
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedElement != null)
            {
                draggedElement.ReleaseMouseCapture();
                draggedElement = null;
                isDragging = false;
            }

            // Reset the selected element's border to invisible when mouse is released
            if (selectedElement != null)
            {
                selectedElement.BorderBrush = Brushes.Transparent;
                selectedElement.BorderThickness = new Thickness(0);
                selectedElement = null; // Clear the selection
            }

            // Allow mouse up events to pass through
            e.Handled = false;
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

    }
}

   