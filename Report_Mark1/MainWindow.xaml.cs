using DevExpress.XtraReports.UI;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Ribbon;
using WpfLabel = System.Windows.Controls.Label;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using Microsoft.Win32;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using System.IO;
using System.Linq;

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
        private double originalWidth;
        private double originalHeight;
        private EditBox editBox;
        #endregion

        #region Property
        public string SelectedFont { get; set; } = "Calibri";

        #endregion


        #region Constructor
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            report = new XtraReport();
            DetailBand detail = new DetailBand();
            report.Bands.Add(detail);

            this.KeyDown += MainWindow_KeyDown;
            designSurface.PreviewMouseLeftButtonDown += DesignSurface_PreviewMouseLeftButtonDown;
            editBox = new EditBox { Visibility = Visibility.Collapsed };
            dataPanelGrid.Children.Add(editBox);
            editBox.CloseRequested += (s, e) => editBox.Visibility = Visibility.Collapsed;

            fontSizeComboBox.AddHandler(RibbonGalleryItem.PreviewMouseLeftButtonDownEvent,
    new MouseButtonEventHandler(FontSizeComboBox_ItemSelected), true);

        }
        #endregion

        #region LeftSide Controls

        private void AddLabel_Click(object sender, RoutedEventArgs e)
        {
            WpfLabel label = new WpfLabel
            {
                Content = "New Label",
                FontSize = 16,
                Margin = new Thickness(10),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                BorderThickness = new Thickness(1)
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
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1)
            };

            textbox.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            textbox.PreviewMouseMove += Element_MouseMove;
            textbox.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

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
                Background = Brushes.White,
                Margin = new Thickness(5)
            };

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
                        BorderBrush = Brushes.Black,
                        BorderThickness = new Thickness(0.5)
                    };
                    Grid.SetRow(cell, i);
                    Grid.SetColumn(cell, j);
                    tableGrid.Children.Add(cell);
                }
            }

            Border wrapper = new Border
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Child = tableGrid
            };

            wrapper.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            wrapper.PreviewMouseMove += Element_MouseMove;
            wrapper.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

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
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = Brushes.LightYellow,
                Child = new TextBlock
                {
                    Text = "Chart Placeholder",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkSlateGray
                }
            };

            chartBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            chartBorder.PreviewMouseMove += Element_MouseMove;
            chartBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;
            chartBorder.PreviewMouseLeftButtonDown += (s, args) => SelectElement(chartBorder);

            Canvas.SetLeft(chartBorder, 100);
            Canvas.SetTop(chartBorder, 100);
            designSurface.Children.Add(chartBorder);

            SelectElement(chartBorder);

            XRChart chart = new XRChart
            {
                WidthF = 400,
                HeightF = 300
            };
            report.Bands[BandKind.Detail].Controls.Add(chart);

            if (currentData != null)
            {
                editBox.LoadData(currentData, chart);
                editBox.Visibility = Visibility.Visible;
                editBox.Tag = chartBorder; // Link to chartBorder
            }
            else
            {
                MessageBox.Show("Generate data first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void AddBarcode_Click(object sender, RoutedEventArgs e)
        {
            Border barcodeBorder = new Border
            {
                Width = 200,
                Height = 50,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1),
                Background = Brushes.LightGray,
                Child = new TextBlock
                {
                    Text = "Barcode: 123456789",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 14,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.DarkBlue
                }
            };

            barcodeBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            barcodeBorder.PreviewMouseMove += Element_MouseMove;
            barcodeBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            barcodeBorder.PreviewMouseLeftButtonDown += (s, args) => SelectElement(barcodeBorder);

            Canvas.SetLeft(barcodeBorder, 100);
            Canvas.SetTop(barcodeBorder, 150);
            designSurface.Children.Add(barcodeBorder);

            SelectElement(barcodeBorder);

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
            var filePicker = new Microsoft.Win32.OpenFileDialog();
            filePicker.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp;*.gif";

            bool? result = filePicker.ShowDialog();
            if (result == true)
            {
                string filePath = filePicker.FileName;

                System.Windows.Controls.Image imageControl = new System.Windows.Controls.Image
                {
                    Width = 150,
                    Height = 150,
                    Stretch = Stretch.Fill,
                    Source = new BitmapImage(new Uri(filePath))
                };

                Border imageBorder = new Border
                {
                    Width = 150,
                    Height = 150,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    Child = imageControl
                };

                System.Windows.Point resizeStartPosition = new System.Windows.Point();
                double originalWidth = imageBorder.Width;
                double originalHeight = imageBorder.Height;
                bool isResizing = false;

                imageBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
                imageBorder.PreviewMouseMove += Element_MouseMove;
                imageBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

                imageBorder.MouseMove += (s, args) =>
                {
                    if (!imageBorder.IsMouseCaptured)
                    {
                        if (IsMouseOverResizeHandle(args, imageBorder))
                        {
                            imageBorder.Cursor = Cursors.SizeNWSE;
                        }
                        else
                        {
                            imageBorder.Cursor = Cursors.Arrow;
                        }
                    }
                };

                imageBorder.MouseLeave += (s, args) =>
                {
                    imageBorder.Cursor = Cursors.Arrow;
                };

                imageBorder.MouseLeftButtonDown += (s, args) =>
                {
                    if (IsMouseOverResizeHandle(args, imageBorder))
                    {
                        imageBorder.CaptureMouse();
                        resizeStartPosition = args.GetPosition(designSurface);
                        originalWidth = imageBorder.Width;
                        originalHeight = imageBorder.Height;
                        isResizing = true;
                        args.Handled = true;
                    }
                };

                imageBorder.MouseMove += (s, args) =>
                {
                    if (imageBorder.IsMouseCaptured && isResizing)
                    {
                        System.Windows.Point currentPos = args.GetPosition(designSurface);
                        double deltaX = currentPos.X - resizeStartPosition.X;
                        double deltaY = currentPos.Y - resizeStartPosition.Y;
                        double newWidth = originalWidth + deltaX;
                        double newHeight = originalHeight + deltaY;
                        if (newWidth > 50) imageBorder.Width = newWidth;
                        if (newHeight > 50) imageBorder.Height = newHeight;
                        imageControl.Width = imageBorder.Width;
                        imageControl.Height = imageBorder.Height;
                    }
                };

                imageBorder.MouseLeftButtonUp += (s, args) =>
                {
                    if (imageBorder.IsMouseCaptured)
                    {
                        imageBorder.ReleaseMouseCapture();
                        imageBorder.Cursor = Cursors.Arrow;
                        isResizing = false;
                    }
                };

                Canvas.SetLeft(imageBorder, 120);
                Canvas.SetTop(imageBorder, 120);
                designSurface.Children.Add(imageBorder);

                SelectElement(imageBorder);

                XRPictureBox image = new XRPictureBox
                {
                    ImageUrl = filePath,
                    Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage,
                    WidthF = (float)imageBorder.Width,
                    HeightF = (float)imageBorder.Height
                };

                report.Bands[BandKind.Detail].Controls.Add(image);
            }
        }
        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Create a SaveFileDialog to let the user choose the PDF file location
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "PDF files (*.pdf)|*.pdf",
                    Title = "Save Report as PDF",
                    FileName = "Report.pdf"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Create a new PDF document
                    using (PdfDocument document = new PdfDocument())
                    {
                        // Create a new page
                        PdfPage page = document.AddPage();
                        page.Width = XUnit.FromPoint(designSurface.Width);
                        page.Height = XUnit.FromPoint(designSurface.Height);

                        // Get graphics context for drawing
                        using (XGraphics gfx = XGraphics.FromPdfPage(page))
                        {
                            // Render the Canvas to a bitmap
                            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                                (int)designSurface.Width,
                                (int)designSurface.Height,
                                96, // DPI
                                96, // DPI
                                PixelFormats.Pbgra32);

                            // Ensure the Canvas is measured and arranged
                            designSurface.Measure(new Size(designSurface.Width, designSurface.Height));
                            designSurface.Arrange(new Rect(0, 0, designSurface.Width, designSurface.Height));
                            renderBitmap.Render(designSurface);

                            // Convert the bitmap to a format PdfSharp can use
                            using (MemoryStream stream = new MemoryStream())
                            {
                                BitmapEncoder encoder = new PngBitmapEncoder();
                                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                                encoder.Save(stream);
                                stream.Position = 0;

                                // Wrap MemoryStream in a Func<Stream>
                                Func<Stream> streamFunc = () => stream;

                                // Draw the image onto the PDF page
                                XImage image = XImage.FromStream(streamFunc);
                                gfx.DrawImage(image, 0, 0, designSurface.Width, designSurface.Height);
                            }
                        }

                        // Save the PDF document
                        document.Save(saveFileDialog.FileName);
                    }

                    MessageBox.Show("PDF exported successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Deletion section
        private void DeleteElement()
        {
            if (selectedElement == null)
            {
                MessageBox.Show("Please select an element to delete.",
                    "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (designSurface.Children.Contains(selectedElement))
            {
                designSurface.Children.Remove(selectedElement);
            }

            if (elementMapping.ContainsKey(selectedElement))
            {
                XRControl control = elementMapping[selectedElement];
                if (report.Bands[BandKind.Detail].Controls.Contains(control))
                {
                    report.Bands[BandKind.Detail].Controls.Remove(control);
                }
                elementMapping.Remove(selectedElement);
            }

            selectedElement = null;

            MessageBox.Show("Element deleted successfully.",
                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        #endregion

        #region Dataside


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
            designSurface.Children.Clear();

            if (currentData != null)
            {
                UserControl reportControl = null;
                string selectedType = (dataTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

                switch (selectedType)
                {
                    case "Invoice":
                        reportControl = new Invoice(); // Make sure Invoice.xaml is a UserControl
                        (reportControl as Invoice)?.LoadReportData(currentData.AsEnumerable());
                        break;

                    case "Sales":
                        reportControl = new Sales(); // Make sure Invoice.xaml is a UserControl
                        (reportControl as Sales)?.LoadReportData(currentData.AsEnumerable());
                        break;
                    case "Product":
                        reportControl = new ReportTemplate();
                        (reportControl as ReportTemplate)?.LoadReportData(currentData.AsEnumerable());
                        break;
                    case "Quotation":
                        reportControl = new Quotation();
                        (reportControl as Quotation)?.LoadReportData(currentData.AsEnumerable());
                        break;

                    default:
                        MessageBox.Show("Unsupported report type.");
                        return;
                }

                if (reportControl != null)
                {
                    reportControl.Width = 793;
                    reportControl.Height = 1122;

                    Canvas.SetLeft(reportControl, 10);
                    Canvas.SetTop(reportControl, 10);
                    designSurface.Children.Add(reportControl);

                    AttachClickEventsRecursively(reportControl);

                    Border headerBorder = reportControl.FindName("headerBorder") as Border;
                    Border tableBorder = reportControl.FindName("tableBorder") as Border;
                    Border footerBorder = reportControl.FindName("footerBorder") as Border;

                    SetupBorderEvents(headerBorder);
                    SetupBorderEvents(tableBorder);
                    SetupBorderEvents(footerBorder);

                    MessageBox.Show("Report loaded into canvas.",
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Please generate data before selecting a report.",
                    "No Data", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SetupBorderEvents(Border border)
        {
            if (border == null) return;

            border.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
            border.PreviewMouseMove += Element_MouseMove;
            border.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;

            border.MouseMove += (s, args) =>
            {
                if (!border.IsMouseCaptured && IsMouseOverResizeHandle(args, border))
                {
                    border.Cursor = Cursors.SizeNWSE;
                }
                else if (!border.IsMouseCaptured)
                {
                    border.Cursor = Cursors.Arrow;
                }
            };

            border.MouseLeave += (s, args) => border.Cursor = Cursors.Arrow;
        }


        void AttachClickEventsRecursively(DependencyObject parent)
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child is TextBlock || child is TextBox || child is Label || child is Border || child is Grid)
                {
                    if (child is UIElement uiElement)
                    {
                        uiElement.PreviewMouseLeftButtonDown -= CanvasElement_Click;
                        uiElement.PreviewMouseLeftButtonDown += CanvasElement_Click;
                    }
                }

                AttachClickEventsRecursively(child);
            }
        }
        #endregion

        #region Canvas
        private void CanvasElement_Click(object sender, MouseButtonEventArgs e)
        {
            ClearAllSelections();

            Border selected = sender as Border;
            if (selected != null)
            {
                selected.BorderBrush = Brushes.Blue;
            }

            e.Handled = true;
        }

        public void SelectElement(UIElement element)
        {
            foreach (UIElement child in designSurface.Children)
            {
                if (child is Control ctrl)
                {
                    ctrl.BorderBrush = Brushes.Transparent;
                }
                else if (child is Border border)
                {
                    border.BorderBrush = border.Tag != null ? (Brush)border.Tag : Brushes.Transparent;
                    border.BorderThickness = new Thickness(1);
                }
                else if (child is ReportTemplate reportTemplate)
                {
                    var headerBorder = reportTemplate.FindName("headerBorder") as Border;
                    var tableBorder = reportTemplate.FindName("tableBorder") as Border;
                    var footerBorder = reportTemplate.FindName("footerBorder") as Border;

                    if (headerBorder != null)
                    {
                        headerBorder.BorderBrush = headerBorder.Tag != null ? (Brush)headerBorder.Tag : Brushes.Transparent;
                        headerBorder.BorderThickness = new Thickness(1);
                    }
                    if (tableBorder != null)
                    {
                        tableBorder.BorderBrush = tableBorder.Tag != null ? (Brush)tableBorder.Tag : Brushes.Transparent;
                        tableBorder.BorderThickness = new Thickness(1);
                    }
                    if (footerBorder != null)
                    {
                        footerBorder.BorderBrush = footerBorder.Tag != null ? (Brush)footerBorder.Tag : Brushes.Transparent;
                        footerBorder.BorderThickness = new Thickness(1);
                    }
                }
            }

            selectedElement = element;

            if (selectedElement is Control selectedCtrl)
            {
                selectedCtrl.BorderBrush = Brushes.Blue;
            }
            else if (selectedElement is Border selectedBorder)
            {
                if (selectedBorder.Tag == null)
                {
                    selectedBorder.Tag = selectedBorder.BorderBrush;
                }
                selectedBorder.BorderBrush = Brushes.Blue;
                selectedBorder.BorderThickness = new Thickness(2);
            }
        }

        private void ClearAllSelections()
        {
            foreach (var child in designSurface.Children)
            {
                if (child is Border border)
                {
                    border.BorderBrush = Brushes.Transparent;
                }
            }
        }

        private void DesignSurface_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Check if the click is directly on the canvas (empty space)
            if (e.OriginalSource == designSurface || !(e.OriginalSource is UIElement))
            {
                SelectElement(null); // Deselect any selected element
                e.Handled = true; // Mark the event as handled to prevent bubbling
            }
        }

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && selectedElement != null)
            {
                DeleteElement();
                e.Handled = true;
            }
        }
        #endregion

        #region Left side


        private bool IsMouseOverResizeHandle(MouseEventArgs args, Border border)
        {
            System.Windows.Point position = args.GetPosition(border);
            double borderWidth = border.ActualWidth;
            double borderHeight = border.ActualHeight;

            return position.X >= borderWidth - 5 && position.X <= borderWidth &&
                   position.Y >= borderHeight - 5 && position.Y <= borderHeight;
        }

        private void ShowPreview_Click(object sender, RoutedEventArgs e)
        {
            PreviewWindow preview = new PreviewWindow(report);
            preview.ShowDialog();
        }



        private void Tool_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border border)
            {
                string toolType = border.Tag as string;
                DragDrop.DoDragDrop(border, toolType, DragDropEffects.Copy);
            }
        }
        #endregion

        #region Zoom Functionality
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (canvasScaleTransform != null && scrollViewer != null)
            {
                double zoomValue = e.NewValue;
                double oldValue = e.OldValue;

                if (oldValue <= 0) oldValue = 1;

                canvasScaleTransform.ScaleX = zoomValue;
                canvasScaleTransform.ScaleY = zoomValue;

                if (zoomLabel != null)
                {
                    zoomLabel.Text = $"{(int)(zoomValue * 100)}%";
                }

                double newHorizontalOffset = scrollViewer.HorizontalOffset * zoomValue / oldValue;
                double newVerticalOffset = scrollViewer.VerticalOffset * zoomValue / oldValue;

                newHorizontalOffset = Math.Max(0, Math.Min(newHorizontalOffset, scrollViewer.ScrollableWidth));
                newVerticalOffset = Math.Max(0, Math.Min(newVerticalOffset, scrollViewer.ScrollableHeight));

                scrollViewer.ScrollToHorizontalOffset(newHorizontalOffset);
                scrollViewer.ScrollToVerticalOffset(newVerticalOffset);
            }
        }
        #endregion

        #region Mouse Events
        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var clickedElement = sender as UIElement;
            if (clickedElement != null)
            {
                // Check if the event originates from a DataGrid element
                if (e.OriginalSource is DependencyObject source)
                {
                    DependencyObject parent = source;
                    while (parent != null)
                    {
                        if (parent is DataGrid || parent is DataGridColumnHeader || parent is DataGridRow)
                        {
                            e.Handled = false; // Let DataGrid handle resizing
                            return;
                        }
                        parent = VisualTreeHelper.GetParent(parent);
                    }
                }

                SelectElement(clickedElement);

                draggedElement = clickedElement;

                if (clickedElement is Border border)
                {
                    if (IsMouseOverResizeHandle(e, border))
                    {
                        border.CaptureMouse();
                        mouseOffset = e.GetPosition(designSurface);
                        originalWidth = border.ActualWidth;
                        originalHeight = border.ActualHeight;
                        isResizing = true;
                        return;
                    }

                    isDragging = true;
                    mouseOffset = e.GetPosition(designSurface);
                    border.CaptureMouse();
                }
                else
                {
                    isDragging = true;
                    mouseOffset = e.GetPosition(designSurface);
                    clickedElement.CaptureMouse();
                }

                e.Handled = true;
            }
        }

        private void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && draggedElement != null)
            {
                System.Windows.Point position = e.GetPosition(designSurface);
                Canvas.SetLeft(draggedElement, position.X - mouseOffset.X);
                Canvas.SetTop(draggedElement, position.Y - mouseOffset.Y);
            }
            else if (isResizing && draggedElement is Border border)
            {
                System.Windows.Point currentPos = e.GetPosition(designSurface);
                double deltaX = currentPos.X - mouseOffset.X;
                double deltaY = currentPos.Y - mouseOffset.Y;
                double newWidth = originalWidth + deltaX;
                double newHeight = originalHeight + deltaY;

                if (newWidth > 50) border.Width = newWidth;
                if (newHeight > 50) border.Height = newHeight;

                if (border.Name == "tableBorder")
                {
                    var dataGrid = FindVisualChild<DataGrid>(border);
                    if (dataGrid != null)
                    {
                        dataGrid.Width = newWidth - 20;
                        dataGrid.Height = newHeight - 20;
                    }
                }
            }
        }

        private void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (draggedElement != null)
            {
                draggedElement.ReleaseMouseCapture();
                if (isResizing)
                {
                    isResizing = false;
                }
                else if (isDragging)
                {
                    isDragging = false;
                }
                draggedElement = null;
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child != null && child is T)
                    return (T)child;
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
        #endregion

        #region Ribbon

        public static IEnumerable<T> FindChildrenOfType<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child is T t)
                    yield return t;

                foreach (var childOfChild in FindChildrenOfType<T>(child))
                    yield return childOfChild;
            }
        }

        private void btnBold_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement == null)
            {
                MessageBox.Show("No element selected.");
                return;
            }

            // If a specific TextBlock or TextBox is selected, only toggle that
            var selectedTextBlock = selectedElement as TextBlock;
            var selectedTextBox = selectedElement as TextBox;
            var selectedRichTextBox = selectedElement as RichTextBox;

            if (selectedTextBlock != null)
            {
                // Toggle bold for the selected TextBlock only
                selectedTextBlock.FontWeight = selectedTextBlock.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
            }
            else if (selectedTextBox != null)
            {
                // Toggle bold for the selected TextBox only
                selectedTextBox.FontWeight = selectedTextBox.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
            }
            else if (selectedRichTextBox != null)
            {
                // Apply bold only to selected text in the RichTextBox (if any selection exists)
                var selection = selectedRichTextBox.Selection;
                var range = selection.IsEmpty
                    ? new TextRange(selectedRichTextBox.Document.ContentStart, selectedRichTextBox.Document.ContentEnd)
                    : new TextRange(selection.Start, selection.End);

                var currentFontWeight = range.GetPropertyValue(TextElement.FontWeightProperty);
                if (currentFontWeight != DependencyProperty.UnsetValue && currentFontWeight is FontWeight fw)
                {
                    range.ApplyPropertyValue(TextElement.FontWeightProperty,
                        fw == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold);
                }
                else
                {
                    range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                }
            }
            else
            {
                // If no specific element is selected, apply bold to all elements in the selected section (header/footer/table)
                var textBlocks = FindChildrenOfType<TextBlock>(selectedElement);
                foreach (var tb in textBlocks)
                {
                    tb.FontWeight = tb.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
                }

                var textBoxes = FindChildrenOfType<TextBox>(selectedElement);
                foreach (var tb in textBoxes)
                {
                    tb.FontWeight = tb.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
                }

                var richTextBoxes = FindChildrenOfType<RichTextBox>(selectedElement);
                foreach (var rtb in richTextBoxes)
                {
                    var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    var currentFontWeight = range.GetPropertyValue(TextElement.FontWeightProperty);
                    if (currentFontWeight != DependencyProperty.UnsetValue && currentFontWeight is FontWeight fw)
                    {
                        range.ApplyPropertyValue(TextElement.FontWeightProperty,
                            fw == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold);
                    }
                    else
                    {
                        range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
                    }
                }
            }
        }


        private void btnItalic_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement == null)
            {
                MessageBox.Show("No element selected.");
                return;
            }

            // Try direct casting
            var selectedTextBlock = selectedElement as TextBlock;
            var selectedTextBox = selectedElement as TextBox;
            var selectedRichTextBox = selectedElement as RichTextBox;

            // If none found, try to find a single child of each type
            if (selectedTextBlock == null)
            {
                var textBlocks = FindChildrenOfType<TextBlock>(selectedElement);
                if (textBlocks.Count() == 1)
                    selectedTextBlock = textBlocks.First();
            }

            if (selectedTextBox == null)
            {
                var textBoxes = FindChildrenOfType<TextBox>(selectedElement);
                if (textBoxes.Count() == 1)
                    selectedTextBox = textBoxes.First();
            }

            if (selectedRichTextBox == null)
            {
                var richTextBoxes = FindChildrenOfType<RichTextBox>(selectedElement);
                if (richTextBoxes.Count() == 1)
                    selectedRichTextBox = richTextBoxes.First();
            }

            // Apply Italic style
            if (selectedTextBlock != null)
            {
                selectedTextBlock.FontStyle = selectedTextBlock.FontStyle == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic;
            }
            else if (selectedTextBox != null)
            {
                selectedTextBox.FontStyle = selectedTextBox.FontStyle == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic;
            }
            else if (selectedRichTextBox != null)
            {
                var selection = selectedRichTextBox.Selection;
                var range = selection.IsEmpty
                    ? new TextRange(selectedRichTextBox.Document.ContentStart, selectedRichTextBox.Document.ContentEnd)
                    : new TextRange(selection.Start, selection.End);

                var currentFontStyle = range.GetPropertyValue(TextElement.FontStyleProperty);
                if (currentFontStyle != DependencyProperty.UnsetValue && currentFontStyle is FontStyle fs)
                {
                    range.ApplyPropertyValue(TextElement.FontStyleProperty,
                        fs == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic);
                }
                else
                {
                    range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                }
            }
            else
            {
                // Apply to all child elements if it's a container with multiple
                var textBlocks = FindChildrenOfType<TextBlock>(selectedElement);
                foreach (var tb in textBlocks)
                {
                    tb.FontStyle = tb.FontStyle == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic;
                }

                var textBoxes = FindChildrenOfType<TextBox>(selectedElement);
                foreach (var tb in textBoxes)
                {
                    tb.FontStyle = tb.FontStyle == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic;
                }

                var richTextBoxes = FindChildrenOfType<RichTextBox>(selectedElement);
                foreach (var rtb in richTextBoxes)
                {
                    var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    var currentFontStyle = range.GetPropertyValue(TextElement.FontStyleProperty);
                    if (currentFontStyle != DependencyProperty.UnsetValue && currentFontStyle is FontStyle fs)
                    {
                        range.ApplyPropertyValue(TextElement.FontStyleProperty,
                            fs == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic);
                    }
                    else
                    {
                        range.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
                    }
                }
            }
        }


        private void btnUnderline_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement == null)
            {
                MessageBox.Show("No element selected.");
                return;
            }

            var selectedTextBlock = selectedElement as TextBlock;
            var selectedTextBox = selectedElement as TextBox;
            var selectedRichTextBox = selectedElement as RichTextBox;

            // Try to find single children if needed
            if (selectedTextBlock == null)
            {
                var textBlocks = FindChildrenOfType<TextBlock>(selectedElement);
                if (textBlocks.Count()== 1)
                    selectedTextBlock = textBlocks.First();
            }

            if (selectedTextBox == null)
            {
                var textBoxes = FindChildrenOfType<TextBox>(selectedElement);
                if (textBoxes.Count() == 1)
                    selectedTextBox = textBoxes.First();
            }

            if (selectedRichTextBox == null)
            {
                var richTextBoxes = FindChildrenOfType<RichTextBox>(selectedElement);
                if (richTextBoxes.Count() == 1)
                    selectedRichTextBox = richTextBoxes.First();
            }

            if (selectedTextBlock != null)
            {
                selectedTextBlock.TextDecorations = selectedTextBlock.TextDecorations == TextDecorations.Underline
                    ? null : TextDecorations.Underline;
            }
            else if (selectedTextBox != null)
            {
                selectedTextBox.TextDecorations = selectedTextBox.TextDecorations == TextDecorations.Underline
                    ? null : TextDecorations.Underline;
            }
            else if (selectedRichTextBox != null)
            {
                var selection = selectedRichTextBox.Selection;
                var range = selection.IsEmpty
                    ? new TextRange(selectedRichTextBox.Document.ContentStart, selectedRichTextBox.Document.ContentEnd)
                    : new TextRange(selection.Start, selection.End);

                var currentDecor = range.GetPropertyValue(Inline.TextDecorationsProperty);
                if (currentDecor != DependencyProperty.UnsetValue && currentDecor is TextDecorationCollection tdc &&
                    tdc == TextDecorations.Underline)
                {
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                }
                else
                {
                    range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                }
            }
            else
            {
                // Handle all children if it's a container
                var textBlocks = FindChildrenOfType<TextBlock>(selectedElement);
                foreach (var tb in textBlocks)
                {
                    tb.TextDecorations = tb.TextDecorations == TextDecorations.Underline ? null : TextDecorations.Underline;
                }

                var textBoxes = FindChildrenOfType<TextBox>(selectedElement);
                foreach (var tb in textBoxes)
                {
                    tb.TextDecorations = tb.TextDecorations == TextDecorations.Underline ? null : TextDecorations.Underline;
                }

                var richTextBoxes = FindChildrenOfType<RichTextBox>(selectedElement);
                foreach (var rtb in richTextBoxes)
                {
                    var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    var currentDecor = range.GetPropertyValue(Inline.TextDecorationsProperty);
                    if (currentDecor != DependencyProperty.UnsetValue && currentDecor is TextDecorationCollection tdc &&
                        tdc == TextDecorations.Underline)
                    {
                        range.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                    }
                    else
                    {
                        range.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
                    }
                }
            }
        }


        private void FontGalleryItem_Selected(object sender, RoutedEventArgs e)
        {
            if (selectedElement == null)
                return;

            if (sender is RibbonGalleryItem item && item.Content is string fontName)
            {
                var fontFamily = new FontFamily(fontName);
                ApplyFontFamilyToSelectedElement(fontFamily);
            }
        }


        private void ApplyFontFamilyToSelectedElement(FontFamily fontFamily)
        {
            if (selectedElement is TextBlock textBlock)
            {
                textBlock.FontFamily = fontFamily;
            }
            else if (selectedElement is TextBox textBox)
            {
                textBox.FontFamily = fontFamily;
            }
            else if (selectedElement is RichTextBox richTextBox)
            {
                var selection = richTextBox.Selection;
                var range = selection.IsEmpty
                    ? new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd)
                    : new TextRange(selection.Start, selection.End);

                range.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
            }
            else
            {
                // Apply to children if selected element is a container
                foreach (var tb in FindChildrenOfType<TextBlock>(selectedElement))
                {
                    tb.FontFamily = fontFamily;
                }
                foreach (var tb in FindChildrenOfType<TextBox>(selectedElement))
                {
                    tb.FontFamily = fontFamily;
                }
                foreach (var rtb in FindChildrenOfType<RichTextBox>(selectedElement))
                {
                    var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    range.ApplyPropertyValue(TextElement.FontFamilyProperty, fontFamily);
                }
            }
        }


        private void FontSizeComboBox_ItemSelected(object sender, MouseButtonEventArgs e)
        {
            var item = FindAncestor<RibbonGalleryItem>(e.OriginalSource as DependencyObject);
            if (item == null || selectedElement == null)
                return;

            if (double.TryParse(item.Content.ToString(), out double fontSize))
            {
                ApplyFontSizeToSelectedElement(fontSize);
            }
        }

        private T FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null && !(current is T))
            {
                current = VisualTreeHelper.GetParent(current);
            }
            return current as T;
        }


        private void ApplyFontSizeToSelectedElement(double fontSize)
        {
            if (selectedElement is TextBlock textBlock)
            {
                textBlock.FontSize = fontSize;
            }
            else if (selectedElement is TextBox textBox)
            {
                textBox.FontSize = fontSize;
            }
            else if (selectedElement is RichTextBox richTextBox)
            {
                var selection = richTextBox.Selection;
                var range = selection.IsEmpty
                    ? new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd)
                    : new TextRange(selection.Start, selection.End);

                range.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
            }
            else
            {
                // Handle containers like Border, Grid, etc.
                var textBlocks = FindChildrenOfType<TextBlock>(selectedElement);
                foreach (var tb in textBlocks)
                    tb.FontSize = fontSize;

                var textBoxes = FindChildrenOfType<TextBox>(selectedElement);
                foreach (var tb in textBoxes)
                    tb.FontSize = fontSize;

                var richTextBoxes = FindChildrenOfType<RichTextBox>(selectedElement);
                foreach (var rtb in richTextBoxes)
                {
                    var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                    range.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                }
            }
        }




        //************************************************************************************************************************************************************//


        private void btnAlignLeft_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement == null)
                return;

            AlignTextRecursive(selectedElement);
        }


      

        private void AlignTextRecursive(DependencyObject parent)
        {
            if (parent is TextBlock tb)
                tb.TextAlignment = TextAlignment.Left;

            else if (parent is TextBox tx)
                tx.TextAlignment = TextAlignment.Left;

            else if (parent is RichTextBox rtb)
            {
                var range = new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
                range.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
            }

            // Traverse children recursively
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                AlignTextRecursive(child);
            }
        }



        #endregion

    }
}