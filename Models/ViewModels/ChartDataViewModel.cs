namespace Biblio.Models.ViewModels
{
    // كلاس مساعد بسيط عشان نبعت البيانات للـ Charts
    public class ChartDataViewModel
    {
        public List<int> Ids { get; set; } = new List<int>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<decimal> Data { get; set; } = new List<decimal>();
    }

    // كلاس مساعد للـ charts الزمنية (الشهور والسنين)
    public class TimeChartDataViewModel
    {
        public List<string> Labels { get; set; } = new List<string>(); // e.g., ["Jan", "Feb", "Mar"]
        public List<ChartSeries> Series { get; set; } = new List<ChartSeries>(); // e.g., [ { Name = "Collection A", Data = [5, 0, 2] }, { Name = "Collection B", Data = [1, 3, 0] } ]
    }

    public class ChartSeries
    {
        public string Name { get; set; }
        public List<int> Data { get; set; } = new List<int>();
    }
}