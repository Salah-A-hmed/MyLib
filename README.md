# Biblio - Smart Library Management System 📚

Biblio is a comprehensive, SaaS-ready web application designed for managing personal or small-to-medium libraries. It streamlines book tracking, borrowing cycles, visitor management, and membership subscriptions.

Built with **ASP.NET Core MVC (.NET 9)**, adhering to modern clean code principles and secure architecture.

## ✨ Key Features

### 📖 Core Library Management
* **Book Inventory:** Add books manually or fetch details automatically via **Google Books API**.
* **Search:** Find books using the Google Books API and add them to your collection.
* **Collections:** Organize books into custom collections (e.g., Sci-Fi, Favorites).
* **Borrowing System:** Track checked-out books, due dates, and returns.
* **Visitor Management:** Maintain a database of library visitors and their borrowing history.

### 💰 Monetization & Subscriptions (SaaS)
* **Stripe Integration:** Secure payment processing for subscription plans.
* **Tiered Access:**
    * **Reader (Free):** Basic access to manage personal books.
    * **Librarian (Pro):** Unlocks Lending System, Visitor Management, and Advanced Analytics.
* **Flexible Billing:** Monthly and Yearly subscription options with auto-downgrade handling.

### 🤖 Automation & Background Services
* **Smart Notifications:** Automated system alerts for overdue books and low stock.
* **Fine Calculation:** Daily background jobs to calculate and update overdue fines.
* **Email Service:** Integrated SMTP email notifications for account verification and password resets.

### 📊 Dashboards & Analytics
* **Interactive Charts:** Visual insights using Plotly and Google Charts.
* **Admin Panel:** Full control over users, roles, and platform-wide statistics.
* **User Profile:** Personalized dashboard showing reading stats, borrowing history, and subscription status.

## 🛠️ Tech Stack

* **Framework:** ASP.NET Core 9 MVC
* **Database:** Microsoft SQL Server (Entity Framework Core Code-First)
* **Authentication:** ASP.NET Core Identity (Roles & Claims)
* **Payments:** Stripe API
* **Frontend:** HTML5, CSS3, Bootstrap 5, Tailwind CSS (Utility classes), JavaScript (ES6+)
* **External APIs:** Google Books API

## 🚀 Getting Started

Follow these instructions to get the project up and running on your local machine.

### Prerequisites
* .NET 9.0 SDK
* SQL Server (LocalDB or Express)
* Stripe Account (for payments)
* Gmail Account (for email sending - optional)

### Installation

1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/salah-a-hmed/mylib.git](https://github.com/salah-a-hmed/mylib.git)
    cd mylib
    ```

2.  **Configure Settings:**
    Update `appsettings.json` or use **User Secrets** (Recommended) with your connection string and API keys:

    ```json
    {
      "ConnectionStrings": {
        "DefaultConnection": "Server=YOUR_SERVER;Database=Biblio_DB;Trusted_Connection=True;TrustServerCertificate=True;"
      },
      "Stripe": {
        "SecretKey": "sk_test_...",
        "PublishableKey": "pk_test_...",
        "PriceId_Monthly": "price_...",
        "PriceId_Yearly": "price_..."
      },
      "EmailSettings": {
        "Host": "smtp.gmail.com",
        "Port": 587,
        "FromEmail": "your-email@gmail.com",
        "Password": "your-app-password"
      }
    }
    ```

3.  **Apply Migrations:**
    Create the database and apply the schema.
    ```bash
    dotnet ef database update
    ```

4.  **Run the Application:**
    ```bash
    dotnet run
    ```

### 👤 Default Admin User (Seeded)
The application seeds a default admin user on the first run:
* **Email:** `admin@biblio.com`
* **Password:** `Admin@123`

## 🤝 Contributing

contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License. See the [LICENSE](https://www.google.com/search?q=LICENSE) file for details.