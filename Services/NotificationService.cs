using Biblio.Data;
using Biblio.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Biblio.Services
{
    // (جديد) دي الخدمة اللي هتشتغل في الخلفية
    public class NotificationService : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly ILogger<NotificationService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private const double FinePerDay = 5.0; // (ثابت الغرامة)
        private const int OverdueThresholdCount = 10; // (رقم 3)
        private const int LongTermOverdueDays = -30; // (رقم 4)
        private const int VisitorFineThreshold = 150; // (رقم 7)
        private const int DueSoonDays = 2; // (رقم 2)

        // New fields for daily stock-check flag and reset timer
        private bool _stockChecked = false; // default false
        private Timer? _stockResetTimer;
        private readonly object _stockLock = new();

        public NotificationService(ILogger<NotificationService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Background Service is starting.");
            // (هنخليها تشتغل كل ساعة)
            _timer = new Timer(DoWork, null, TimeSpan.FromMinutes(1), TimeSpan.FromHours(1));

            //For testing purposes, you can uncomment the line below to run it immediately without delay
            //_timer = new Timer(DoWork, null, TimeSpan.Zero, Timeout.InfiniteTimeSpan);

            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            _logger.LogInformation("Notification Background Service is checking for updates...");

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                await GenerateNotificationsAsync(context);
            }
        }

        private async Task GenerateNotificationsAsync(AppDbContext context)
        {
            try
            {
                var today = DateTime.Now.Date;

                // --- (تعديل 1) تحديث الـ Overdue الأول ---
                // (ده لوجيك تحديث الغرامات، لازم يتنفذ الأول)
                var itemsToMakeOverdue = await context.Borrowings
                    .Where(b => b.Status == BorrowingStatus.Borrowed && b.DueDate.Date < today)
                    .ToListAsync();

                if (itemsToMakeOverdue.Any())
                {
                    foreach (var item in itemsToMakeOverdue)
                    {
                        item.Status = BorrowingStatus.Overdue;
                        item.FineAmount = ((today - item.DueDate.Date).Days) * FinePerDay;
                        context.Update(item);
                    }
                    await context.SaveChangesAsync(); // (احفظ الغرامات قبل ما نبعت الإشعارات)
                }

                // --- (تعديل 2) هنجيب كل المستخدمين اللي عندهم أي حاجة ---
                var allUserIds = await context.Users.Select(u => u.Id).ToListAsync();

                foreach (var userId in allUserIds)
                {
                    // هنجيب كل استعارات المستخدم ده (النشطة بس)
                    var userBorrowings = await context.Borrowings
                        .Include(b => b.Book)
                        .Include(b => b.Visitor) // (مهم عشان إشعار 6 و 7)
                        .Where(b => b.UserId == userId && (b.Status == BorrowingStatus.Borrowed || b.Status == BorrowingStatus.Overdue))
                        .ToListAsync();

                    if (!userBorrowings.Any()) continue; // (لو معندوش استعارات، شوف المستخدم اللي بعده)

                    // --- تنفيذ الـ 7 طلبات ---

                    // (طلب 1) إشعار "Overdue" (للي لسه حالته متغيرة)
                    foreach (var item in userBorrowings.Where(b => itemsToMakeOverdue.Select(i => i.ID).Contains(b.ID)))
                    {
                        await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Alert,
                            $"Book '{item.Book.Title}' is now OVERDUE.",
                            $"/Borrowings/Details/{item.ID}",
                            item.BookID, item.VisitorID, item.ID);
                    }

                    // (طلب 2) إشعار "Due Soon"
                    foreach (var item in userBorrowings.Where(b => b.Status == BorrowingStatus.Borrowed &&
                                                               b.DueDate.Date >= today &&
                                                               b.DueDate.Date <= today.AddDays(DueSoonDays)))
                    {
                        await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Warning,
                            $"Book '{item.Book.Title}' is due soon (on {item.DueDate.ToShortDateString()}).",
                            $"/Borrowings/Details/{item.ID}",
                            item.BookID, item.VisitorID, item.ID);
                    }

                    // (طلب 4) إشعار "Long-Term Overdue"
                    foreach (var item in userBorrowings.Where(b => b.Status == BorrowingStatus.Overdue &&
                                                               b.DueDate.Date < today.AddDays(LongTermOverdueDays)))
                    {
                        await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Alert,
                            $"ESCALATION: Book '{item.Book.Title}' has been overdue for over 30 days!",
                            $"/Borrowings/Details/{item.ID}",
                            item.BookID, item.VisitorID, item.ID);
                    }

                    // (طلب 3) إشعار "Overdue Threshold" (للمستخدم)
                    var totalOverdueCount = userBorrowings.Count(b => b.Status == BorrowingStatus.Overdue);
                    if (totalOverdueCount >= OverdueThresholdCount)
                    {
                        await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Alert,
                            $"Warning: You have {totalOverdueCount} books currently overdue.",
                            "/Borrowings/Index", null, null, null);
                    }

                    // (طلب 6 و 7) تجميع حسب الزائر
                    var borrowingsByVisitor = userBorrowings.GroupBy(b => b.Visitor);
                    foreach (var group in borrowingsByVisitor)
                    {
                        var visitor = group.Key;
                        var visitorBorrowings = group.ToList();

                        // (طلب 6) "All Visitor Borrowings Overdue"
                        if (visitorBorrowings.Any() && visitorBorrowings.All(b => b.Status == BorrowingStatus.Overdue))
                        {
                            await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Alert,
                                $"Alert! All borrowed books for visitor '{visitor.Name}' are currently overdue.",
                                $"/Visitors/Details/{visitor.ID}",
                                null, visitor.ID, null);
                        }

                        // (طلب 7) "Visitor Fine Threshold"
                        var totalFines = visitorBorrowings.Sum(b => b.FineAmount);
                        if (totalFines > VisitorFineThreshold)
                        {
                            await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Alert,
                               $"Alert! Visitor '{visitor.Name}' has accumulated over {VisitorFineThreshold:C0} in fines.",
                               $"/Visitors/Details/{visitor.ID}",
                               null, visitor.ID, null);
                        }
                    }
                }

                // (طلب 5) "Out of Stock" using a boolean flag to ensure it runs once per 24 hours
                bool shouldRunStockCheck = false;
                lock (_stockLock)
                {
                    if (!_stockChecked)
                    {
                        // mark as checked and schedule reset in 24 hours
                        _stockChecked = true;
                        shouldRunStockCheck = true;

                        // dispose previous reset timer if any
                        try
                        {
                            _stockResetTimer?.Dispose();
                        }
                        catch { /* ignore */ }

                        // schedule a one-shot timer to reset the flag after 24 hours
                        _stockResetTimer = new Timer(state =>
                        {
                            lock (_stockLock)
                            {
                                _stockChecked = false;
                            }
                            try
                            {
                                _stockResetTimer?.Dispose();
                            }
                            catch { /* ignore */ }
                        }, null, TimeSpan.FromDays(1), Timeout.InfiniteTimeSpan);
                    }
                }

                if (shouldRunStockCheck)
                {
                    var allUserIdsForStockCheck = await context.Books.Select(b => b.UserId).Distinct().ToListAsync();
                    foreach (var userId in allUserIdsForStockCheck)
                    {
                        var outOfStockBooks = await context.Books
                            .Where(b => b.UserId == userId && (b.TotalCopies - b.CheckedOutCopies) <= 0)
                            .CountAsync();

                        if (outOfStockBooks > 0)
                        {
                            await CreateNotificationIfNotExistsAsync(context, userId, NotificationType.Information,
                               $"Daily Summary: You have {outOfStockBooks} books currently out of stock.",
                               "/Books/Index", // (ممكن نعدل اللينك ده لفلتر مخصص بعدين)
                               null, null, null);
                        }
                    }
                }

                await context.SaveChangesAsync();
                _logger.LogInformation("Notification check completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating notifications.");
            }
        }

        // الدالة المساعدة (هنخليها تتأكد من الرسالة)
        private async Task CreateNotificationIfNotExistsAsync(
            AppDbContext context, string userId, NotificationType type,
            string message, string linkUrl,
            int? bookId, int? visitorId, int? borrowingId)
        {
            // (هنتأكد إن مفيش إشعار "غير مقروء" بنفس الرسالة)
            var existingNotification = await context.Notifications
                .AnyAsync(n => n.UserId == userId &&
                               n.Status == NotificationStatus.Unread &&
                               n.Message == message);

            if (!existingNotification)
            {
                var notification = new Notification
                {
                    UserId = userId,
                    Type = type,
                    Message = message,
                    LinkUrl = linkUrl,
                    BookID = bookId,
                    VisitorID = visitorId,
                    BorrowingID = borrowingId
                };
                await context.Notifications.AddAsync(notification);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Notification Background Service is stopping.");
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
            try
            {
                _stockResetTimer?.Dispose();
            }
            catch { /* ignore */ }
        }
    }
}