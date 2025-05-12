using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Report_Mark1
{
    public partial class Invoice : UserControl
    {
        public Invoice()
        {
            InitializeComponent();
        }

        public void LoadReportData(IEnumerable<DataRow> rows)
        {
            if (rows == null || !rows.Any())
                return;

            var dataTable = rows.First().Table;
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToList();

            var items = rows.Select(row =>
            {
                // Map the columns dynamically based on the DataTable structure
                string description = "N/A";
                double quantity = 1; // Default quantity if not available
                double unitPrice = 0;

                // Determine the column mapping based on the DataTable's columns
                if (columnNames.Contains("Description"))
                {
                    description = row["Description"]?.ToString() ?? "N/A";
                }
                else if (columnNames.Contains("Invoice No")) // For Invoice data type
                {
                    description = $"Invoice #{row["Invoice No"]} - {row["Customer"]}";
                }
                else if (columnNames.Contains("Bill No")) // For Sales data type
                {
                    description = $"Bill #{row["Bill No"]}";
                }
                else if (columnNames.Contains("Product ID")) // For Product data type
                {
                    description = $"{row["Product ID"]} - {row["Name"]}";
                }
                else if (columnNames.Contains("Quote No")) // For Quotation data type
                {
                    description = $"Quote #{row["Quote No"]} - {row["Requested By"]}";
                }

                // Map quantity (default to 1 if not available)
                if (columnNames.Contains("Quantity"))
                {
                    quantity = Convert.ToDouble(row["Quantity"]);
                }

                // Map unit price
                if (columnNames.Contains("UnitPrice"))
                {
                    unitPrice = Convert.ToDouble(row["UnitPrice"]);
                }
                else if (columnNames.Contains("Total")) // For Sales data type
                {
                    unitPrice = Convert.ToDouble(row["Total"]);
                }
                else if (columnNames.Contains("Price")) // For Product data type
                {
                    unitPrice = Convert.ToDouble(row["Price"]);
                }
                else if (columnNames.Contains("Amount")) // For Invoice data type
                {
                    unitPrice = Convert.ToDouble(row["Amount"]);
                }
                else if (columnNames.Contains("Estimate")) // For Quotation data type
                {
                    unitPrice = Convert.ToDouble(row["Estimate"]);
                }

                // Calculate item total (renamed to avoid conflict)
                double itemTotal = quantity * unitPrice;

                return new
                {
                    Description = description,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Total = itemTotal // Renamed to itemTotal
                };
            }).ToList();

            InvoiceItemsGrid.ItemsSource = items;

            double subtotal = items.Sum(i => i.Total);
            double tax = subtotal * 0.18;
            double total = subtotal + tax; // This 'total' is now fine as there's no conflict

            SubtotalText.Text = $"₹{subtotal:0.00}";
            TaxText.Text = $"₹{tax:0.00}";
            TotalText.Text = $"₹{total:0.00}";
        }

        private void UpdateTotalText(string label, double value)
        {
            // Find the totals StackPanel (vertical, width=300)
            var totalsStackPanel = FindTotalsStackPanel(this);
            if (totalsStackPanel == null)
                return;

            // Iterate through child StackPanels (horizontal) to find the matching label
            foreach (var child in totalsStackPanel.Children.OfType<StackPanel>())
            {
                if (child.Orientation == Orientation.Horizontal &&
                    child.Children[0] is TextBlock labelBlock &&
                    labelBlock.Text.TrimEnd(':').Equals(label.TrimEnd(':'), StringComparison.OrdinalIgnoreCase) &&
                    child.Children[1] is TextBlock valueBlock)
                {
                    valueBlock.Text = $"₹{value:0.00}";
                    break;
                }
            }
        }

        private StackPanel FindTotalsStackPanel(DependencyObject parent)
        {
            // Find the main StackPanel containing the InvoiceItemsGrid
            var mainStackPanel = FindVisualChild<StackPanel>(parent, sp => sp.Orientation == Orientation.Vertical && sp.Margin == new Thickness(20));
            if (mainStackPanel == null)
                return null;

            // Find the totals StackPanel (vertical, width=300, HorizontalAlignment=Right)
            foreach (var child in mainStackPanel.Children.OfType<StackPanel>())
            {
                if (child.Orientation == Orientation.Vertical &&
                    child.Width == 300 &&
                    child.HorizontalAlignment == HorizontalAlignment.Right)
                {
                    return child;
                }
            }

            return null;
        }

        private T FindVisualChild<T>(DependencyObject parent, Func<T, bool> predicate = null) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild && (predicate == null || predicate(typedChild)))
                {
                    return typedChild;
                }

                var result = FindVisualChild<T>(child, predicate);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}