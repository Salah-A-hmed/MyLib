using Biblio.Data;
using Biblio.Models;
using Biblio.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;

public class BookFilterService : IBookFilterService
{
    private readonly AppDbContext _context;

    public BookFilterService(AppDbContext context)
    {
        _context = context;
    }

    public IQueryable<Book> GetFilteredBooks(string userId, string sortOrder, string statusFilter, string searchString, int? collectionId)
    {
        var books = _context.Books
            .Include(b => b.User)
            .Where(b => b.UserId == userId)
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            books = books.Where(b =>
                b.Title.Contains(searchString) ||
                (b.Author != null && b.Author.Contains(searchString))
            );
        }

        // Apply collection filter
        if (collectionId.HasValue && collectionId > 0)
        {
            books = books.Where(b => b.Collections.Any(bc => bc.CollectionID == collectionId));
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
        {
            books = books.Where(b => b.Status == statusFilter);
        }

        // Apply sorting
        books = sortOrder switch
        {
            "Title_desc" => books.OrderByDescending(b => b.Title),
            "Author_desc" => books.OrderByDescending(b => b.Author),
            "Publisher_desc" => books.OrderByDescending(b => b.Publisher),
            "Category_desc" => books.OrderByDescending(b => b.Category),
            "Rating_desc" => books.OrderByDescending(b => b.Rating),

            "Author" => books.OrderBy(b => b.Author),
            "Publisher" => books.OrderBy(b => b.Publisher),
            "Category" => books.OrderBy(b => b.Category),
            "Rating" => books.OrderBy(b => b.Rating),

            // default
            _ => books.OrderBy(b => b.Title)
        };

        return books;
    }
}