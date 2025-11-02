using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;
using Wrap_nRoll.Services;

namespace Wrap_nRoll.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CheckoutController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IEmailService _emailService;
        private const string CartSessionKey = "Cart";

        public CheckoutController(AppDbContext context, UserManager<User> userManager, IEmailService emailService)
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
        }

        // 🔒 Session helpers
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void ClearCart()
        {
            HttpContext.Session.Remove(CartSessionKey);
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Warning"] = "Your cart is empty!";
                return RedirectToAction("Index", "Store");
            }

            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder()
        {
            var cart = GetCart();
            if (!cart.Any())
            {
                TempData["Warning"] = "Your cart is empty!";
                return RedirectToAction("Index", "Store");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "You must be logged in to place an order.";
                return RedirectToAction("Login", "Account");
            }

            // ✅ Create order
            var order = new Order
            {
                CustomerId = user.Id,
                Customer = user,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cart.Sum(c => c.Price * c.Quantity),
                OrderItems = new List<OrderItem>()
            };

            // ✅ Add each wrap from the cart (including fillings)
            foreach (var item in cart)
            {
                var wrap = await _context.Wraps.FindAsync(item.WrapId);

                order.OrderItems.Add(new OrderItem
                {
                    WrapId = item.WrapId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Price,
                    Customizations = item.Customizations ?? string.Join(", ", item.Ingredients ?? new List<string>()),
                    Wrap = wrap
                });
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // ✅ Clear cart after successful checkout
            ClearCart();

            // ✅ Email to business
            string businessEmail = "wrapnroll57@gmail.com";
            string businessSubject = $"🆕 New Order #{order.OrderId}";
            string businessMessage = $@"
New order received from {user.Name} (#{order.OrderId})

Total: R{order.TotalAmount:F2}
Items:
{string.Join("\n", order.OrderItems.Select(i =>
    $"- {i.Wrap?.Name} x{i.Quantity} ({(i.Customizations ?? "Default fillings")})"
))}
";

            await _emailService.SendEmailAsync(businessEmail, businessSubject, businessMessage);

            // ✅ Email to customer
            if (!string.IsNullOrEmpty(user.Email))
            {
                string customerSubject = $"✅ Order Confirmation #{order.OrderId}";
                string customerMessage = $@"
Hi {user.Name},

Your order #{order.OrderId} has been placed successfully!

Total: R{order.TotalAmount:F2}

Items:
{string.Join("\n", order.OrderItems.Select(i =>
    $"- {i.Wrap?.Name} x{i.Quantity} ({(i.Customizations ?? "Default fillings")})"
))}

Thank you for choosing Wrap 'n Roll!
";

                await _emailService.SendEmailAsync(user.Email, customerSubject, customerMessage);
            }

            TempData["Success"] = "Your order has been placed successfully!";
            return RedirectToAction("Details", "CustomerOrders", new { id = order.OrderId });
        }
    }
}
