using System.Collections.Generic;

namespace Biblio.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        // KPIs
        public int TotalUsers { get; set; }
        public int TotalBooks { get; set; }
        public int TotalCollections { get; set; }

        // User Charts
        public ChartDataViewModel UserCollectionChart { get; set; } // Pie
        public ChartDataViewModel UserBookCountChart { get; set; }  // Bar

        // Platform Charts
        public ChartDataViewModel PlatformBooksTimeline { get; set; }
        public ChartDataViewModel PlatformCollectionsTimeline { get; set; }

        public AdminDashboardViewModel()
        {
            UserCollectionChart = new ChartDataViewModel();
            UserBookCountChart = new ChartDataViewModel();
            PlatformBooksTimeline = new ChartDataViewModel();
            PlatformCollectionsTimeline = new ChartDataViewModel();
        }
    }
}