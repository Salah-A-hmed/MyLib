using System.ComponentModel.DataAnnotations.Schema;

namespace Biblio.Models
{
    public class Collection
    {
         // Properties 
         public int ID { get; set; }
         public string Name { get; set; }
         public string? Description { get; set; }

          //Relations 
          // Book_Collection_Relationship 
          public ICollection<BookCollection> Books { get; set; } = new List<BookCollection>();

          // User_Collection_Relationship 
          [ForeignKey("User")]
          public string? UserId { get; set; }
          public AppUser? User { get; set; }

        }
    }
