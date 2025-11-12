using System.Collections.Generic;

namespace Biblio.Models.ViewModels
{
    public class CollectionDashboardViewModel
    {
        // 1. KPIs (المستطيلات)
        public int TotalUniqueBooks { get; set; }
        public int TotalCopies { get; set; }

        // 2. Pie Chart (Collection vs Book Count)
        public ChartDataViewModel CollectionBookCountChart { get; set; }

        // 3. Column Charts (الزمنية)
        public TimeChartDataViewModel MonthlyAddedChart { get; set; }
        public TimeChartDataViewModel YearlyAddedChart { get; set; }

        // 4. Bar Chart (النسخ)
        public ChartDataViewModel CollectionCopiesChart { get; set; }

        // 5. Bar Chart (القيمة المالية)
        public ChartDataViewModel CollectionValueChart { get; set; }

        public CollectionDashboardViewModel()
        {
            CollectionBookCountChart = new ChartDataViewModel();
            MonthlyAddedChart = new TimeChartDataViewModel();
            YearlyAddedChart = new TimeChartDataViewModel();
            CollectionCopiesChart = new ChartDataViewModel();
            CollectionValueChart = new ChartDataViewModel();
        }
    }
}