using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;

namespace Wrap_nRoll.Controllers
{
    [Authorize(Roles = "Customer")]
    public class CartController : Controller
    {
        private readonly AppDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        private List<string> GetDefaultIngredients(string wrapName)
        {
            wrapName = wrapName?.ToLower() ?? "";

            if (wrapName.Contains("classic"))
            {
                return new List<string> { "Lettuce", "Tomato", "Cucumber", "Sweet Heat Sauce", "Creamy Chilli Sauce" };
            }
            else if (wrapName.Contains("fully loaded"))
            {
                return new List<string> { "Fries", "Lettuce", "Tomato", "Cucumber", "Sweet Heat Sauce", "Creamy Chilli Sauce" };
            }

            return new List<string> { "Lettuce", "Tomato", "Cucumber", "Sweet Heat Sauce", "Creamy Chilli Sauce" };
        }

        // ✅ Update Quantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int wrapId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.WrapId == wrapId);
            if (item != null)
            {
                item.Quantity = Math.Clamp(quantity, 1, 20);
                SaveCart(cart);
                TempData["Success"] = $"{item.Name} quantity updated.";
            }
            return RedirectToAction("Index");
        }

        // ✅ Update Fillings / Customizations
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCustomizations(int wrapId, [FromForm] List<string> selectedIngredients)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.WrapId == wrapId);

            if (item != null)
            {
                // if user deselected all fillings, fallback to default
                var finalIngredients = (selectedIngredients != null && selectedIngredients.Any())
                    ? selectedIngredients
                    : GetDefaultIngredients(item.Name);

                item.Ingredients = finalIngredients;
                item.Customizations = string.Join(", ", finalIngredients);

                SaveCart(cart);
                TempData["Success"] = $"{item.Name} customized successfully!";
            }

            return RedirectToAction("Index");
        }

        // ✅ Add Item to Cart
        // ✅ Add Item to Cart (Improved)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int wrapId, int quantity = 1, [FromForm] List<string> selectedIngredients = null)
        {
            var wrap = _context.Wraps.Find(wrapId);
            if (wrap == null) return NotFound();

            var cart = GetCart();
            var existingItem = cart.FirstOrDefault(c => c.WrapId == wrapId);

            if (existingItem != null)
            {
                existingItem.Quantity += quantity;
            }
            else
            {
                var ingredients = selectedIngredients != null && selectedIngredients.Any()
                    ? selectedIngredients
                    : GetDefaultIngredients(wrap.Name);

                cart.Add(new CartItem
                {
                    WrapId = wrap.WrapId,
                    Name = wrap.Name,
                    Quantity = quantity,
                    Price = wrap.Price,
                    ImageUrl = wrap.ImageUrl,
                    Ingredients = ingredients,
                    Customizations = string.Join(", ", ingredients)
                });
            }

            SaveCart(cart);
            TempData["Success"] = "Your wrap has been added to the cart!";
            return RedirectToAction("Index", "Store");
        }


        // ✅ Remove Item
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int wrapId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.WrapId == wrapId);
            if (item != null)
            {
                cart.Remove(item);
                SaveCart(cart);
                TempData["Success"] = $"{item.Name} removed from your cart.";
            }
            return RedirectToAction("Index");
        }

        // ✅ View Cart
        public IActionResult Index()
        {
            var cart = GetCart();

            foreach (var item in cart)
            {
                if (item.Ingredients == null || !item.Ingredients.Any())
                {
                    item.Ingredients = GetDefaultIngredients(item.Name);
                    item.Customizations = string.Join(", ", item.Ingredients);
                }
            }

            return View(cart);
        }

        // ✅ Session Helpers
        private List<CartItem> GetCart()
        {
            var cartJson = HttpContext.Session.GetString(CartSessionKey);
            return string.IsNullOrEmpty(cartJson)
                ? new List<CartItem>()
                : JsonConvert.DeserializeObject<List<CartItem>>(cartJson) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartSessionKey, JsonConvert.SerializeObject(cart));
        }
        // ✅ Returns number of items in cart
        [HttpGet]
        public int GetCartCount()
        {
            var cart = GetCart();
            return cart.Sum(c => c.Quantity);
        }

    }
}
