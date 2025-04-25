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


        #endregion


        #region Constructor


        public MainWindow()
        {
            InitializeComponent();

            // Initialize an empty report with a detail band
            report = new XtraReport();
            DetailBand detail = new DetailBand();
            report.Bands.Add(detail);
        }


        #endregion

        private void SelectElement(UIElement element)
        {
            selectedElement = element;
            // Optional: add highlight or border to show it's selected
        }


        #region Dataside
        private void GenerateReport_Click(object sender, RoutedEventArgs e)
        {
            string selectedType = (dataTypeComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            DateTime? fromDate = fromDatePicker.SelectedDate;
            DateTime? toDate = toDatePicker.SelectedDate;

            if (string.IsNullOrEmpty(selectedType) || fromDate == null || toDate == null)
            {
                MessageBox.Show("Please select data type and date range.", "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            MessageBox.Show($"Demo {selectedType} data generated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SelectReport_Click(object sender, RoutedEventArgs e)
        {
            // Clear any existing content in the design surface
            designSurface.Children.Clear();

            // Check if there is any data
            if (currentData != null)
            {
                // Create the table grid as before
                Grid tableGrid = new Grid
                {
                    ShowGridLines = true,
                    Margin = new Thickness(10),
                    Background = System.Windows.Media.Brushes.White
                };

                // Define columns
                for (int col = 0; col < currentData.Columns.Count; col++)
                {
                    tableGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
                }

                // Add header row
                tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                for (int col = 0; col < currentData.Columns.Count; col++)
                {
                    TextBox header = new TextBox
                    {
                        Text = currentData.Columns[col].ColumnName,
                        FontWeight = FontWeights.Bold,
                        Padding = new Thickness(5),
                        Background = System.Windows.Media.Brushes.LightGray,
                        BorderBrush = System.Windows.Media.Brushes.Black,
                        BorderThickness = new Thickness(1),
                        IsReadOnly = true // Make header readonly
                    };

                    Grid.SetRow(header, 0);
                    Grid.SetColumn(header, col);
                    tableGrid.Children.Add(header);
                }

                // Add data rows with editable cells
                for (int row = 0; row < currentData.Rows.Count; row++)
                {
                    tableGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                    for (int col = 0; col < currentData.Columns.Count; col++)
                    {
                        TextBox cell = new TextBox
                        {
                            Text = currentData.Rows[row][col].ToString(),
                            Padding = new Thickness(5),
                            BorderBrush = System.Windows.Media.Brushes.Gray,
                            BorderThickness = new Thickness(0.5),
                            Background = System.Windows.Media.Brushes.White
                        };

                        // Make the cell editable
                        cell.TextChanged += (s, args) =>
                        {
                            // Update the data when the user edits the cell
                            currentData.Rows[row][col] = cell.Text;
                        };

                        Grid.SetRow(cell, row + 1);
                        Grid.SetColumn(cell, col);
                        tableGrid.Children.Add(cell);
                    }
                }

                // Add to canvas
                Canvas.SetLeft(tableGrid, 10);
                Canvas.SetTop(tableGrid, 10);

                Border wrapper = new Border
                {
                    BorderBrush = System.Windows.Media.Brushes.Black,
                    BorderThickness = new Thickness(1),
                    Child = tableGrid
                };

                // Make it draggable
                wrapper.MouseLeftButtonDown += Element_MouseLeftButtonDown;
                wrapper.MouseMove += Element_MouseMove;
                wrapper.MouseLeftButtonUp += Element_MouseLeftButtonUp;

                // Optional: select the element on click
                wrapper.MouseLeftButtonDown += (s, args) => SelectElement(wrapper);

                Canvas.SetLeft(wrapper, 10);
                Canvas.SetTop(wrapper, 10);
                designSurface.Children.Add(wrapper);

                MessageBox.Show("Table loaded into canvas.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

            wrapper.MouseLeftButtonDown += CanvasElement_Click;

            Canvas.SetLeft(wrapper, 50);
            Canvas.SetTop(wrapper, 50);
            designSurface.Children.Add(wrapper);
        }



        private void AddChart_Click(object sender, RoutedEventArgs e)
        {
            XRChart chart = new XRChart
            {
                WidthF = 400,
                HeightF = 300
            };

            report.Bands[BandKind.Detail].Controls.Add(chart);
        }

        private void AddBarcode_Click(object sender, RoutedEventArgs e)
        {
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
            XRPictureBox image = new XRPictureBox
            {
                ImageUrl = "https://via.placeholder.com/150",
                Sizing = DevExpress.XtraPrinting.ImageSizeMode.StretchImage,
                WidthF = 150,
                HeightF = 150
            };

            report.Bands[BandKind.Detail].Controls.Add(image);
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







        // BOLD
        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement is TextBlock tb)
            {
                tb.FontWeight = tb.FontWeight == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold;
            }
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement is TextBlock tb)
            {
                tb.FontStyle = tb.FontStyle == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic;
            }
        }


        // ADD ROW to selected table
        private void AddRow_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement is Grid table)
            {
                int rowIndex = table.RowDefinitions.Count;
                table.RowDefinitions.Add(new RowDefinition());

                for (int col = 0; col < table.ColumnDefinitions.Count; col++)
                {
                    var cell = new TextBlock
                    {
                        Text = $"R{rowIndex + 1}C{col + 1}",
                        Margin = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(cell, rowIndex);
                    Grid.SetColumn(cell, col);
                    table.Children.Add(cell);
                }
            }
        }

        private void AddColumn_Click(object sender, RoutedEventArgs e)
        {
            if (selectedElement is Grid table)
            {
                int colIndex = table.ColumnDefinitions.Count;
                table.ColumnDefinitions.Add(new ColumnDefinition());

                for (int row = 0; row < table.RowDefinitions.Count; row++)
                {
                    var cell = new TextBlock
                    {
                        Text = $"R{row + 1}C{colIndex + 1}",
                        Margin = new Thickness(5),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, colIndex);
                    table.Children.Add(cell);
                }
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








