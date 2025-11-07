using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json.Serialization;

namespace Biblio.Models.ViewModels
{
    public class BookEditViewModel
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? Publisher { get; set; }
        public int? PublishYear { get; set; }
        public int? Pages { get; set; }
        public string? ISBN { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Category { get; set; } // Fiction, Non-Fiction, Science, History, Biography, etc.
        public decimal? Rating { get; set; } // from 0 to 5
        public string? Review { get; set; }
        public string? Status { get; set; } // abandoned, Not Begun, In Progress, Completed
        public int? StockCount { get; set; } = 1;
        public List<int> SelectedCollectionIds { get; set; } = new List<int>();
        public List<SelectListItem> Collections { get; set; } = new List<SelectListItem>();
        [JsonIgnore]
        public SelectList StatusOptions { get; set; } = new SelectList(new List<string>
        {
            "No Status",
            "Not Begun",
            "In Progress",
            "Completed",
            "Abandoned"
        });
    }
}
