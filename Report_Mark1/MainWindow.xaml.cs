using DevExpress.XtraReports.UI;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using System;
using DevExpress.Xpf.Bars;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Windows.Input;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Ribbon;

namespace Report_Mark1
{
    public partial class MainWindow : Window
    {
        #region Member Variables

        private XtraReport report;
        private DataTable currentData;
        private Dictionary<UIElement, XRControl> elementMapping = new Dictionary<UIElement, XRControl>();
        private UIElement selectedElement = null;
        private UIElement draggedElement = null;        
        private bool isDragging = false;
        private System.Windows.Point mouseOffset;
        private Border imageBorder;
        private System.Windows.Controls.Image imageControl;
        private bool isResizing = false;



        #endregion


        public string SelectedFont { get; set; } = "Calibri";

        #region Constructor


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            // Initialize an empty report with a detail band
            report = new XtraReport();
            DetailBand detail = new DetailBand();
            report.Bands.Add(detail);
           
        }


        #endregion



        #region Dataside

        private void SelectElement(UIElement element)
        {
            selectedElement = element;

        }


        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string selectedType = (dataTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime? fromDate = fromDatePicker.SelectedDate;
            DateTime? toDate = toDatePicker.SelectedDate;

            if (string.IsNullOrEmpty(selectedType) || fromDate == null || toDate == null)
            {
                MessageBox.Show("Please select data type and date range.", 
                    "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);

                return;
            }

            // Generate demo data
            currentData = new DataTable();

            switch (selectedType)
            {
                case "Sales":
                    currentData.Columns.Add("Bill No");
                    currentData.Columns.Add("Date");
                    currentData.Columns.Add("Total");

                    currentData.Rows.Add("S001", "2025-04-01", "500");
                    currentData.Rows.Add("S002", "2025-04-02", "650");
                    break;

                case "Product":
                    currentData.Columns.Add("Product ID");
                    currentData.Columns.Add("Name");
                    currentData.Columns.Add("Price");

                    currentData.Rows.Add("P001", "Cake", "300");
                    currentData.Rows.Add("P002", "Bread", "50");
                    break;

                case "Invoice":
                    currentData.Columns.Add("Invoice No");
                    currentData.Columns.Add("Customer");
                    currentData.Columns.Add("Amount");

                    currentData.Rows.Add("INV001", "John", "700");
                    currentData.Rows.Add("INV002", "Doe", "800");
                    break;

                case "Quotation":
                    currentData.Columns.Add("Quote No");
                    currentData.Columns.Add("Requested By");
                    currentData.Columns.Add("Estimate");

                    currentData.Rows.Add("Q001", "Manager", "900");
                    currentData.Rows.Add("Q002", "Client", "1200");
                    break;
            }

            dataPreviewGrid.ItemsSource = currentData.DefaultView;

            MessageBox.Show($"Demo {selectedType} data generated.", 
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void SelectReport_Click(object sender, RoutedEventArgs e)
        {
            // Clear any existing content in the design surface
            designSurface.Children.Clear();

            // Check if there is any data
            if (currentData != null)
            {
                // Create a new instance of ReportTemplate
                ReportTemplate reportTemplate = new ReportTemplate
                {
                    Width = 793,
                    Height = 1122
                };

                // Load the data into the ReportTemplate
                reportTemplate.LoadReportData(currentData.AsEnumerable());

                // Add the ReportTemplate to the canvas
                Canvas.SetLeft(reportTemplate, 10);
                Canvas.SetTop(reportTemplate, 10);
                designSurface.Children.Add(reportTemplate);

                // Selection (still allow selecting the entire report for other purposes)
                reportTemplate.PreviewMouseLeftButtonDown += (s, args) => SelectElement(reportTemplate);

                MessageBox.Show("Report loaded into canvas.",
                    "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Please generate data before selecting a report.",
                    "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        #endregion



        #region Canvas
        private void CanvasElement_Click(object sender, MouseButtonEventArgs e)
        {
            var clicked = sender as UIElement;

            // Traverse up the visual tree to find the outer Border with a Grid inside
            DependencyObject current = clicked;
            while (current != null)
            {
                if (current is Border border && border.Child is Grid)
                {
                    // Set selection
                    selectedElement = border;

                    // Optional: Clear all other borders' highlight before setting new one
                    foreach (var child in designSurface.Children)
                    {
                        if (child is Border b)
                            b.BorderBrush = System.Windows.Media.Brushes.Black;
                    }

                    border.BorderBrush = System.Windows.Media.Brushes.Blue;
                    border.BorderThickness = new Thickness(2);
                    break;
                }

                current = VisualTreeHelper.GetParent(current);
            }
        }


        #endregion



        #region Left side
        private void AddLabel_Click(object sender, RoutedEventArgs e)
        {
            TextBlock label = new TextBlock
            {
                Text = "New Label",
                FontSize = 16,
                Margin = new Thickness(10),
                Background = System.Windows.Media.Brushes.Transparent
            };

            label.MouseLeftButtonDown += Element_MouseLeftButtonDown;
            label.MouseMove += Element_MouseMove;
            label.MouseLeftButtonUp += Element_MouseLeftButtonUp;

            Canvas.SetLeft(label, 50);
            Canvas.SetTop(label, 50);
            designSurface.Children.Add(label);

            SelectElement(label);
        }


        private void AddTextbox_Click(object sender, RoutedEventArgs e)
        {
            TextBox textbox = new TextBox
            {
                Text = "Editable Textbox",
                FontSize = 14,
                Width = 200,
                Height = 60,
                AcceptsReturn = true,
                TextWrapping = TextWrapping.Wrap,
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            // Attach drag handlers
            textbox.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            textbox.PreviewMouseMove += Element_MouseMove;
            textbox.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            // Optional: selection
            textbox.PreviewMouseLeftButtonDown += (s, args) => SelectElement(textbox);

            Canvas.SetLeft(textbox, 50);
            Canvas.SetTop(textbox, 100);
            designSurface.Children.Add(textbox);

            SelectElement(textbox);
        }


        private void AddTable_Click(object sender, RoutedEventArgs e)
        {
            Grid tableGrid = new Grid
            {
                ShowGridLines = true,
                Background = System.Windows.Media.Brushes.White,
                Margin = new Thickness(5)
            };

            // Sample 3x2 Grid
            for (int i = 0; i < 2; i++) tableGrid.RowDefinitions.Add(new RowDefinition());
            for (int j = 0; j < 3; j++) tableGrid.ColumnDefinitions.Add(new ColumnDefinition());

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    TextBox cell = new TextBox
                    {
                        Text = $"R{i}C{j}",
                        Padding = new Thickness(5),
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new Thickness(0.5)
                    };
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    tableGrid.Children.Add(cell);
                }
            }

            Border wrapper = new Border
            {
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = tableGrid
            };

            // Attach drag handlers to the wrapper instead of tableGrid
            wrapper.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            wrapper.PreviewMouseMove += Element_MouseMove;
            wrapper.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            // Also enable selection on click
            wrapper.PreviewMouseLeftButtonDown += (s, args) => SelectElement(wrapper);

            Canvas.SetLeft(wrapper, 50);
            Canvas.SetTop(wrapper, 50);
            designSurface.Children.Add(wrapper);

            SelectElement(wrapper);
        }



        private void AddChart_Click(object sender, RoutedEventArgs e)
        {
            Border chartBorder = new Border
            {
                Width = 400,
                Height = 300,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = System.Windows.Media.Brushes.LightYellow,
                Child = new TextBlock
                {
                    Text = "Chart Placeholder",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.DarkSlateGray
                }
            };

            // Make draggable
            chartBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            chartBorder.PreviewMouseMove += Element_MouseMove;
            chartBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            // Select on click
            chartBorder.PreviewMouseLeftButtonDown += (s, args) => SelectElement(chartBorder);

            Canvas.SetLeft(chartBorder, 100);
            Canvas.SetTop(chartBorder, 100);
            designSurface.Children.Add(chartBorder);

            SelectElement(chartBorder);

            // Optionally: Add actual chart to report backend (invisible for now)
            XRChart chart = new XRChart
            {
                WidthF = 400,
                HeightF = 300
            };
            report.Bands[BandKind.Detail].Controls.Add(chart);
        }



        private void AddBarcode_Click(object sender, RoutedEventArgs e)
        {
            // Create a visible placeholder for WPF design surface
            Border barcodeBorder = new Border
            {
                Width = 200,
                Height = 50,
                BorderBrush = System.Windows.Media.Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = System.Windows.Media.Brushes.LightGray,
                Child = new TextBlock
                {
                    Text = "Barcode: 123456789",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = System.Windows.Media.Brushes.DarkBlue
                }
            };

            // Make it draggable
            barcodeBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            barcodeBorder.PreviewMouseMove += Element_MouseMove;
            barcodeBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            // Make it selectable
            barcodeBorder.PreviewMouseLeftButtonDown += (s, args) => SelectElement(barcodeBorder);

            // Add to canvas
            Canvas.SetLeft(barcodeBorder, 100);
            Canvas.SetTop(barcodeBorder, 150);
            designSurface.Children.Add(barcodeBorder);

            SelectElement(barcodeBorder);

            // Add the actual XRBarcode to the report backend
            XRBarCode barcode = new XRBarCode
            {
                Text = "123456789",
                Symbology = new DevExpress.XtraPrinting.BarCode.Code128Generator(),
                WidthF = 200,
                HeightF = 50
            };

            report.Bands[BandKind.Detail].Controls.Add(barcode);
        }




        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            // Create a file dialog to select an image
            var filePicker = new Microsoft.Win32.OpenFileDialog();
            filePicker.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            bool? result = filePicker.ShowDialog();
            if (result == true)
            {
                // Get the selected image path
                string filePath = filePicker.FileName;

                // Create a visible Image control for the design surface
                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image
                {
                    Width = 150,
                    Height = 150,
                    Stretch = Stretch.Fill,
                    Source = new BitmapImage(new Uri(filePath)) // Use the selected file path
                };

                // Create a Border to wrap the image
                Border imageBorder = new Border
                {
                    Width = 150,
                    Height = 150,
                    BorderBrush = System.Windows.Media.Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Child = imageControl
                };

                // Make the image border draggable
                imageBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
                imageBorder.PreviewMouseMove += Element_MouseMove;
                imageBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

                // Enable resizing by dragging the image border's corner
                imageBorder.MouseLeftButtonDown += (s, args) =>
                {
                    if (IsMouseOverResizeHandle(args))
                    {
                        // Start resizing when mouse is over a corner
                        imageBorder.CaptureMouse();
                        imageBorder.Cursor = Cursors.SizeNWSE;  // Change cursor to resizing
                    }
                };

                imageBorder.MouseMove += (s, args) =>
                {
                    if (imageBorder.IsMouseCaptured)
                    {
                        // Get the mouse position relative to the image border
                        double newWidth = args.GetPosition(imageBorder).X;
                        double newHeight = args.GetPosition(imageBorder).Y;

                        // Ensure width and height are reasonable
                        if (newWidth > 50) imageBorder.Width = newWidth;  // Minimum width
                        if (newHeight > 50) imageBorder.Height = newHeight; // Minimum height

                        imageControl.Width = imageBorder.Width;
                        imageControl.Height = imageBorder.Height;
                    }
                };

                imageBorder.MouseLeftButtonUp += (s, args) =>
                {
                    imageBorder.ReleaseMouseCapture();  // Release the mouse when resizing is done
                    imageBorder.Cursor = Cursors.Arrow; // Reset cursor back to normal
                };

                // Add to the canvas
                Canvas.SetLeft(imageBorder, 120);  // Adjust position as needed
                Canvas.SetTop(imageBorder, 120);   // Adjust position as needed
                designSurface.Children.Add(imageBorder);

                // Optionally, select the element
                SelectElement(imageBorder);

                // Add the actual XRPictureBox to the report backend (for the backend report designer)
                XRPictureBox image = new XRPictureBox
                {
                    ImageUrl = filePath,  // Use the selected image path
                    Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage,
                    WidthF = 150,
                    HeightF = 150
                };

                // Add the image to the report's detail band
                report.Bands[BandKind.Detail].Controls.Add(image);
            }
        }

        private bool IsMouseOverResizeHandle(MouseEventArgs args)
        {
            // Check if the mouse is over the bottom-right corner (resize handle)
            System.Windows.Point position = args.GetPosition(null);  // Specify System.Windows.Point explicitly
            return position.X >= designSurface.ActualWidth - 20 && position.Y >= designSurface.ActualHeight - 20;
        }





        private void ShowPreview_Click(object sender, RoutedEventArgs e)
        {
            PreviewWindow preview = new PreviewWindow(report);
            preview.ShowDialog();
        }


        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            string path = "report_output.pdf";
            report.ExportToPdf(path);
            MessageBox.Show($"Exported to {path}");
        }
        #endregion



        #region Ribbon

        private void FontGallery_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var galleryItem = (sender as RibbonGallery)?.SelectedItem as RibbonGalleryItem;
            if (galleryItem != null)
            {
                SelectedFont = galleryItem.Content.ToString();
            }
        }





        #endregion



        #region Mouse Events



        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggedElement = sender as UIElement;
            mouseOffset = e.GetPosition(draggedElement);
            isDragging = true;
            draggedElement.CaptureMouse();
        }


        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                System.Windows.Point position = e.GetPosition(designSurface);
                Canvas.SetLeft(draggedElement, position.X - mouseOffset.X);
                Canvas.SetTop(draggedElement, position.Y - mouseOffset.Y);
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
        }



        #endregion










    }
}








