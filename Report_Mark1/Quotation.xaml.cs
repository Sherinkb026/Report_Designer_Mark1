using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Controls;

namespace Report_Mark1
{
    public partial class Quotation : UserControl
    {
        public Quotation()
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
                string description = "N/A";
                double quantity = 1;
                double unitPrice = 0;

                if (columnNames.Contains("Description"))
                {
                    description = row["Description"]?.ToString() ?? "N/A";
                }
                else if (columnNames.Contains("Invoice No"))
                {
                    description = $"Invoice #{row["Invoice No"]} - {row["Customer"]}";
                }
                else if (columnNames.Contains("Bill No"))
                {
                    description = $"Bill #{row["Bill No"]}";
                }
                else if (columnNames.Contains("Product ID"))
                {
                    description = $"{row["Product ID"]} - {row["Name"]}";
                }
                else if (columnNames.Contains("Quote No"))
                {
                    description = $"Quote #{row["Quote No"]} - {row["Requested By"]}";
                }

                if (columnNames.Contains("Quantity"))
                {
                    quantity = Convert.ToDouble(row["Quantity"]);
                }

                if (columnNames.Contains("UnitPrice"))
                {
                    unitPrice = Convert.ToDouble(row["UnitPrice"]);
                }
                else if (columnNames.Contains("Total"))
                {
                    unitPrice = Convert.ToDouble(row["Total"]);
                }
                else if (columnNames.Contains("Price"))
                {
                    unitPrice = Convert.ToDouble(row["Price"]);
                }
                else if (columnNames.Contains("Amount"))
                {
                    unitPrice = Convert.ToDouble(row["Amount"]);
                }
                else if (columnNames.Contains("Estimation"))
                {
                    unitPrice = Convert.ToDouble(row["Estimation"]);
                }

                double itemTotal = quantity * unitPrice;

                return new
                {
                    Description = description,
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Estimation = itemTotal
                };
            }).ToList();

            QuotationItemsGrid.ItemsSource = items;

            double subtotal = items.Sum(i => i.Estimation);
            double tax = subtotal * 0.18;
            double total = subtotal + tax;

            SubtotalText.Text = $"₹{subtotal:0.00}";
            TaxText.Text = $"₹{tax:0.00}";
            TotalText.Text = $"₹{total:0.00}";
        }
    }
}