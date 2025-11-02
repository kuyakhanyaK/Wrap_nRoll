using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;
using Wrap_nRoll.Models.ViewModel;

namespace Wrap_nRoll.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CustomerOrdersController : Controller
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private const string CartSessionKey = "Cart"; // Ensure this matches your CartController

        public CustomerOrdersController(AppDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // 🧾 View all customer orders
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var orders = await _dbContext.Orders
                .Where(o => o.CustomerId == user.Id)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Wrap)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 📦 View order details
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var order = await _dbContext.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Wrap)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.CustomerId == user.Id);

            if (order == null) return NotFound();

            return View(order);
        }

        // ✅ Checkout from cart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // Load cart from session
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            var cart = JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
            if (!cart.Any())
            {
                TempData["Error"] = "Your cart is empty!";
                return RedirectToAction("Index", "Cart");
            }

            // Create a new order
            var order = new Order
            {
                CustomerId = user.Id,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cart.Sum(c => c.Price * c.Quantity)
            };

            _dbContext.Orders.Add(order);
            await _dbContext.SaveChangesAsync();

            // Save each cart item as an OrderItem (including customizations/fillings)
            foreach (var cartItem in cart)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    WrapId = cartItem.WrapId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.Price,
                    Customizations = cartItem.Customizations // ✅ fillings/customizations saved here
                };

                _dbContext.OrderItems.Add(orderItem);
            }

            await _dbContext.SaveChangesAsync();

            // Clear cart after checkout
            HttpContext.Session.Remove(CartSessionKey);

            TempData["Success"] = "Order placed successfully! 🎉";
            return RedirectToAction("Details", new { id = order.OrderId });
        }

        // 🥗 Optional: Wrap customization view
        [HttpGet]
        public async Task<IActionResult> CustomizeWrap(int wrapId)
        {
            var wrap = await _dbContext.Wraps
                .Include(w => w.Ingredients)
                .FirstOrDefaultAsync(w => w.WrapId == wrapId);

            if (wrap == null) return NotFound();

            var model = new WrapVM
            {
                WrapId = wrap.WrapId,
                Name = wrap.Name,
                Description = wrap.Description,
                Price = wrap.Price,
                ImageUrl = wrap.ImageUrl,
                IsAvailable = wrap.IsAvailable,
                Ingredients = wrap.Ingredients.Select(i => new IngredientVM
                {
                    IngredientId = i.IngredientId,
                    Name = i.Name,
                    Selected = true
                }).ToList()
            };

            return View(model);
        }

        // 👇 Optional manual add (used if not coming from cart)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToOrder(int wrapId, int quantity, List<string> selectedIngredients)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var wrap = await _dbContext.Wraps.FindAsync(wrapId);
            if (wrap == null) return NotFound();

            var order = await _dbContext.Orders
                .FirstOrDefaultAsync(o => o.CustomerId == user.Id && o.Status == "Pending");

            if (order == null)
            {
                order = new Order
                {
                    CustomerId = user.Id,
                    Status = "Pending",
                    OrderDate = DateTime.Now
                };
                _dbContext.Orders.Add(order);
                await _dbContext.SaveChangesAsync();
            }

            var orderItem = new OrderItem
            {
                WrapId = wrapId,
                Quantity = quantity,
                UnitPrice = wrap.Price,
                Customizations = selectedIngredients != null ? string.Join(", ", selectedIngredients) : null,
                OrderId = order.OrderId
            };

            _dbContext.OrderItems.Add(orderItem);
            await _dbContext.SaveChangesAsync();

            TempData["Success"] = "Your customized wrap has been added to your order!";
            return RedirectToAction("Index");
        }
    }
}
