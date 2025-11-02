using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;
using Wrap_nRoll.Services;

[Authorize(Roles = "Employee,Admin")]
public class EmployeeOrdersController : Controller
{
    private readonly AppDbContext _context;
    private readonly IEmailService _emailService;

    public EmployeeOrdersController(AppDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Wrap)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        ViewBag.Statuses = new[] { "Pending", "Preparing", "Ready", "On the way", "Completed", "Cancelled" };

        return View(orders);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, string status)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == orderId);

        if (order == null) return NotFound();

        var validStatuses = new[] { "Pending", "Preparing", "Ready", "On the way", "Completed", "Cancelled" };
        if (!validStatuses.Contains(status))
        {
            TempData["Error"] = "Invalid status selected!";
            return RedirectToAction(nameof(Index));
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        // Email notification to customer
        if (!string.IsNullOrEmpty(order.Customer?.Email))
        {
            string subject = $"📦 Order #{order.OrderId} Status Update";
            string message = $"Hi {order.Customer.Name},\n\nYour Wrap n Roll order #{order.OrderId} status has been updated to: **{status}**.\n\nThank you for your patience!";
            await _emailService.SendEmailAsync(order.Customer.Email, subject, message);
        }

        TempData["Success"] = $"Order #{order.OrderId} status updated to {status}.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Wrap)
            .FirstOrDefaultAsync(o => o.OrderId == id);

        if (order == null) return NotFound();

        return View(order);
    }
}
