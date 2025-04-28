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
            Canvas.SetTop(tableCanvas, 100); // Position the table below the header

            Canvas.SetLeft(footerCanvas, 520);
            Canvas.SetTop(footerCanvas, 350); // Position the footer further down
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
                    selectedElement.BorderBrush = Brushes.Black;
                    selectedElement.BorderThickness = new Thickness(1);
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
            // Allow mouse up events to pass through
            e.Handled = false;
        }

        #endregion
    }

    public class ReportViewModel
    {
        public DataTable ReportItems { get; set; }
        public string CurrentDate { get; set; }
    }
}