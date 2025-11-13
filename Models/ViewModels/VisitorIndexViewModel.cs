using Biblio.Models;

namespace Biblio.Models.ViewModels
{
    // ده الـ Model الجديد اللي هنستخدمه في صفحة Index
    public class VisitorIndexViewModel
    {
        public Visitor Visitor { get; set; }

        // دي الخاصية الجديدة اللي طلبتها
        public double TotalFines { get; set; }
    }
}