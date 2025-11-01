using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    public class BookCollection
    {
        public int ID { get; set; }
        [ForeignKey("Book")]
        public int BookID { get; set; }
        [ForeignKey("Collection")]
        public int CollectionID { get; set; }
        
        [ForeignKey("User")]
        public string? UserId { get; set; }
        public AppUser? User { get; set; }
        public Book? Book { get; set; }
        public Collection? Collection { get; set; }
    }
}
