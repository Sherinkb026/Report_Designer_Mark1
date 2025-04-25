using DevExpress.XtraReports.Design;
using DevExpress.XtraReports.UI;
using System.Windows;

namespace Report_Mark1
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow(XtraReport report)
        {
            InitializeComponent();
            previewControl.DocumentSource = report;
            report.CreateDocument(); // Generate the report content
        }
    }
}
