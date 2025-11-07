using Biblio.Models;
using System.Linq;
namespace Biblio.Services
{
    public interface IBookFilterService
    {
        IQueryable<Book> GetFilteredBooks(string userId, string sortOrder, string statusFilter, string searchString, int? collectionId);
    }
}

