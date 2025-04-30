using DevExpress.XtraReports.UI;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // Use WPF-specific Media namespace
using System.Data;
using System;
using DevExpress.Xpf.Bars;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Controls.Ribbon;
using WpfLabel = System.Windows.Controls.Label;

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

            report = new XtraReport();
            DetailBand detail = new DetailBand();
            report.Bands.Add(detail);

            this.KeyDown += MainWindow_KeyDown;
        }

        #endregion

        private void MainWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete && selectedElement != null)
            {
                DeleteElement();
                e.Handled = true;
            }
        }

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
                ReportTemplate reportTemplate = new ReportTemplate
                {
                    Width = 793,
                    Height = 1122
                };

                reportTemplate.LoadReportData(currentData.AsEnumerable());

                Canvas.SetLeft(reportTemplate, 10);
                Canvas.SetTop(reportTemplate, 10);
                designSurface.Children.Add(reportTemplate);

                var headerBorder = reportTemplate.FindName("headerBorder") as Border;
                var tableBorder = reportTemplate.FindName("tableBorder") as Border;
                var footerBorder = reportTemplate.FindName("footerBorder") as Border;

                if (headerBorder != null)
                {
                    headerBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
                    headerBorder.PreviewMouseMove += Element_MouseMove;
                    headerBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;
                }
                if (tableBorder != null)
                {
                    tableBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
                    tableBorder.PreviewMouseMove += Element_MouseMove;
                    tableBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;
                }
                if (footerBorder != null)
                {
                    footerBorder.PreviewMouseLeftButtonDown += Element_MouseLeftButtonDown;
                    footerBorder.PreviewMouseMove += Element_MouseMove;
                    footerBorder.PreviewMouseLeftButtonUp += Element_MouseLeftButtonUp;
                }

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

            DependencyObject current = clicked;
            while (current != null)
            {
                if (current is Border border && border.Child is Grid)
                {
                    SelectElement(border);
                    foreach (var child in designSurface.Children)
                    {
                        if (child is Border b)
                            b.BorderBrush = b.Tag != null ? (Brush)b.Tag : Brushes.Black;
                    }
                    border.BorderBrush = Brushes.Blue;
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
                        System.Diagnostics.Debug.WriteLine("Resizing...");
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

        private void ExportPDF_Click(object sender, RoutedEventArgs e)
        {
            string path = "report_output.pdf";
            report.ExportToPdf(path);
            MessageBox.Show($"Exported to {path}");
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

        #region Ribbon

        private void BtnBold_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement is Control ctrl)
            {
                ctrl.FontWeight = (ctrl.FontWeight == FontWeights.Bold) ? FontWeights.Normal : FontWeights.Bold;
            }
        }

        private void FontGallery_SelectionChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var selectedItem = e.NewValue;
            if (selectedItem != null)
            {
                MessageBox.Show($"You selected: {selectedItem}");
            }
        }

        #endregion

        #region Mouse Events

        private void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggedElement = sender as UIElement;
            mouseOffset = e.GetPosition(designSurface);
            isDragging = true;
            draggedElement.CaptureMouse();

            SelectElement(draggedElement);
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