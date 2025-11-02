using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;
using Wrap_nRoll.Models.ViewModel;
using System.IO;

namespace Wrap_nRoll.Controllers
{
    [Authorize(Roles = "Admin")]
    public class WrapController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public WrapController(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // View all wraps
        public async Task<IActionResult> Index()
        {
            var wraps = await _context.Wraps.ToListAsync();
            return View(wraps);
        }

        // Add wrap (GET)
        [HttpGet]
        public IActionResult AddWrap()
        {
            return View();
        }

        // Add wrap (POST) with image upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWrap(WrapVM model)
        {
            if (!ModelState.IsValid) return View(model);

            string imageUrl = null;
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images", "wraps");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imageUrl = "/images/wraps/" + fileName;
            }

            var wrap = new Wrap
            {
                Name = model.Name,
                Description = model.Description,
                Price = model.Price,
                ImageUrl = imageUrl,
                IsAvailable = model.IsAvailable
            };

            _context.Wraps.Add(wrap);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Wrap added successfully!";
            return RedirectToAction("Index");
        }

        // GET: EditWrap
        [HttpGet]
        public async Task<IActionResult> EditWrap(int id)
        {
            var wrap = await _context.Wraps.FindAsync(id);
            if (wrap == null) return NotFound();

            var wrapVM = new WrapVM
            {
                WrapId = wrap.WrapId,
                Name = wrap.Name,
                Description = wrap.Description,
                Price = wrap.Price,
                ImageUrl = wrap.ImageUrl,
                IsAvailable = wrap.IsAvailable
            };

            return View(wrapVM);
        }

        // POST: EditWrap
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWrap(WrapVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var wrap = await _context.Wraps.FindAsync(model.WrapId);
            if (wrap == null) return NotFound();

            wrap.Name = model.Name;
            wrap.Description = model.Description;
            wrap.Price = model.Price;
            wrap.IsAvailable = model.IsAvailable;

            // Handle image upload if a new file is selected
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var uploads = Path.Combine(_webHostEnvironment.WebRootPath, "images", "wraps");
                Directory.CreateDirectory(uploads);

                var fileName = Guid.NewGuid() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                wrap.ImageUrl = "/images/wraps/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Wrap updated successfully!";
            return RedirectToAction("Index");
        }


        // Delete wrap
        public async Task<IActionResult> DeleteWrap(int id)
        {
            var wrap = await _context.Wraps.FindAsync(id);
            if (wrap != null)
            {
                _context.Wraps.Remove(wrap);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Wrap deleted successfully!";
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public async Task<IActionResult> ToggleAvailability(int id)
        {
            var wrap = await _context.Wraps.FindAsync(id);
            if (wrap == null) return NotFound();

            wrap.IsAvailable = !wrap.IsAvailable;
            await _context.SaveChangesAsync();

            // Return JSON for AJAX
            return Json(new { success = true, isAvailable = wrap.IsAvailable });
        }

    }
}
