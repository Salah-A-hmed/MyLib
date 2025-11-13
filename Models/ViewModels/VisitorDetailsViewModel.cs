using Biblio.Models;
using System.Collections.Generic;

namespace Biblio.Models.ViewModels
{
    public class VisitorDetailsViewModel
    {
        // 1. بيانات الكارت الأساسي
        public Visitor Visitor { get; set; }

        // 2. بيانات الـ KPIs
        public int TotalBorrowings { get; set; }
        public double TotalFines { get; set; }

        // 3. بيانات الـ Pie Chart (هنستخدم نفس الكلاس بتاع الداشبورد اللي فات)
        public ChartDataViewModel StatusPieChart { get; set; }

        // 4. بيانات الجدول (قائمة الاستعارات شاملة الكتب)
        public List<Borrowing> Borrowings { get; set; }

        public VisitorDetailsViewModel()
        {
            StatusPieChart = new ChartDataViewModel();
            Borrowings = new List<Borrowing>();
        }
    }
}