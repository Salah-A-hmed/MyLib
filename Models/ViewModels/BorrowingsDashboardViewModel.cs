using Biblio.Models;
using System.Collections.Generic;

namespace Biblio.Models.ViewModels
{
    // الموديل ده هيشيل كل البيانات اللي صفحة Borrowings Index محتاجاها
    public class BorrowingsDashboardViewModel
    {
        // --- بيانات التاب الثاني (Active) ---
        public int KpiTotalActive { get; set; }
        public int KpiDueSoon { get; set; }
        public int KpiTotalOverdue { get; set; }
        public List<Borrowing> ActiveBorrowings { get; set; }

        // --- بيانات التاب الثالث (Returned) ---
        public int KpiTotalReturned { get; set; }
        public int KpiOnTime { get; set; }
        public double KpiTotalCollectedFines { get; set; }
        public List<Borrowing> ReturnedBorrowings { get; set; }

        // --- (Constructor) ---
        public BorrowingsDashboardViewModel()
        {
            ActiveBorrowings = new List<Borrowing>();
            ReturnedBorrowings = new List<Borrowing>();
        }
    }
}