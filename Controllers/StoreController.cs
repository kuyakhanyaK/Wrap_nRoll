using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;

public class StoreController : Controller
{
    private readonly AppDbContext _context;

    public StoreController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        // Show only available wraps
        var wraps = await _context.Wraps
            .Where(w => w.IsAvailable)
            .ToListAsync();

        return View(wraps);
    }
}
