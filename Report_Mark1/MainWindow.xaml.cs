using DevExpress.XtraReports.UI;
using DevExpress.Xpf.Printing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using System;

namespace Report_Mark1
{
    public partial class MainWindow : Window
    {
        private XtraReport report;
        private DataTable currentData;

        public MainWindow()
        {
            InitializeComponent();

            // Initialize an empty report with a detail band
            report = new XtraReport();
            DetailBand detail = new DetailBand();
            report.Bands.Add(detail);
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
            
        }
        #endregion
        #region Left side
        private void AddLabel_Click(object sender, RoutedEventArgs e)
        {
            var label = new TextBlock
            {
                Text = "New Label",
                FontSize = 16,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                Margin = new Thickness(5)
            };

            // You can later make this a Canvas to allow free placement
            Canvas.SetLeft(label, 50);
            Canvas.SetTop(label, 50);

            designSurface.Children.Add(label);
        }


        private void AddTextbox_Click(object sender, RoutedEventArgs e)
        {
            XRRichText textbox = new XRRichText
            {
                WidthF = 200,
                HeightF = 60,
                Html = "<p>Editable Textbox</p>"
            };

            report.Bands[BandKind.Detail].Controls.Add(textbox);
        }

        private void AddTable_Click(object sender, RoutedEventArgs e)
        {
            XRTable table = new XRTable
            {
                WidthF = 300
            };

            XRTableRow row = new XRTableRow();
            row.Cells.Add(new XRTableCell { Text = "Col 1" });
            row.Cells.Add(new XRTableCell { Text = "Col 2" });
            row.Cells.Add(new XRTableCell { Text = "Col 3" });
            table.Rows.Add(row);

            report.Bands[BandKind.Detail].Controls.Add(table);
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





    }
}








