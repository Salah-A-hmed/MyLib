# Edit
# Biblio

Biblio is a comprehensive web application for managing a personal or small library. It allows users to manage their books, collections, borrowings, and visitors. The application is built with ASP.NET Core and Entity Framework Core, and it uses a SQL Server database.

## Features

  * **Book Management:** Add, edit, delete, and view books in your library.
  * **Search:** Find books using the Google Books API and add them to your collection.
  * **Collections:** Organize your books into collections.
  * **Borrowing System:** Keep track of borrowed books, due dates, and returns.
  * **Visitor Management:** Manage a list of people who can borrow books.
  * **Notifications:** Get notified about overdue books and other events.
  * **User Authentication:** Secure user accounts with ASP.NET Core Identity.
  * **User Roles:** Different levels of access for different user types (Admin, Librarian, Reader).

## Technologies Used

  * **Framework:** ASP.NET Core MVC
  * **Database:** Microsoft SQL Server
  * **Object-Relational Mapper (ORM):** Entity Framework Core
  * **Authentication:** ASP.NET Core Identity
  * **Front-End:** HTML, CSS, JavaScript, Bootstrap
  * **API Integration:** Google Books API

## Setup and Installation

1.  **Clone the repository:**
    ```bash
    git clone https://github.com/salah-a-hmed/mylib.git
    ```
2.  **Navigate to the project directory:**
    ```bash
    cd mylib/MyLib-30f2380ef1f96f3965a9a6f23e8961a4d514e2dd
    ```
3.  **Configure the database connection:**
      * Open `appsettings.json`.
      * Modify the `DefaultConnection` string to point to your SQL Server instance.
4.  **Apply database migrations:**
    ```bash
    dotnet ef database update
    ```
5.  **Run the application:**
    ```bash
    dotnet run
    ```
6.  **Access the application:**
      * Open your web browser and navigate to `http://localhost:5035` or `https://localhost:7292`.

## Usage

  * **Register a new account:** Create a new user account to start managing your library.
  * **Add Books:** You can add books manually or use the search feature to find books from the Google Books API.
  * **Create Collections:** Organize your books by creating collections.
  * **Manage Visitors:** Add and manage visitors who can borrow books.
  * **Track Borrowings:** Create borrowing records to track which books are borrowed, by whom, and when they are due.

## Database Schema

The application uses a relational database with the following main entities:

  * **Books:** Stores information about the books in the library.
  * **Collections:** Represents collections of books.
  * **Borrowings:** Tracks the borrowing of books by visitors.
  * **Visitors:** Stores information about the people who borrow books.
  * **Notifications:** Stores notifications for users.
  * **AppUser:** Represents the application's users.

## License

This project is licensed under the MIT License. See the [LICENSE](https://www.google.com/search?q=LICENSE) file for details.
