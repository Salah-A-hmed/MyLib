using Biblio.Models.ViewModels; // (مهم عشان نستخدم الـ ChartDataViewModel)
using System.Collections.Generic;

namespace Biblio.Models.ViewModels
{
    public class CollectionDetailsViewModel
    {
        // 1. بيانات الكولكشن الأساسية
        public int CollectionId { get; set; }
        public string CollectionName { get; set; } = "N/A";
        public string? CollectionDescription { get; set; }

        // 2. KPIs (المستطيلات)
        public int TotalUniqueBooks { get; set; }
        public int TotalCopies { get; set; }

        // 3. Pie Chart (Status)
        public ChartDataViewModel StatusPieChart { get; set; }

        // 4. Bar Chart (Top 10 Copies)
        public ChartDataViewModel CopiesBarChart { get; set; }

        // 5. Bar Chart (Top 10 Priced Books)
        public ChartDataViewModel PriceBarChart { get; set; }

        // 6. Histogram (Rating)
        public ChartDataViewModel RatingHistogram { get; set; }

        // 7. Timeline (Date Added)
        public ChartDataViewModel DateAddedTimeline { get; set; }

        public CollectionDetailsViewModel()
        {
            StatusPieChart = new ChartDataViewModel();
            CopiesBarChart = new ChartDataViewModel();
            PriceBarChart = new ChartDataViewModel();
            RatingHistogram = new ChartDataViewModel();
            DateAddedTimeline = new ChartDataViewModel();
        }
    }
}