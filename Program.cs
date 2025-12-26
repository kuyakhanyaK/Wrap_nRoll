using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Wrap_nRoll.Data;
using Wrap_nRoll.Models;
using Wrap_nRoll.Services;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Add Services -------------------- //

// Controllers with Views
builder.Services.AddControllersWithViews();

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseSqlServer(connectionString);
});

// Identity
builder.Services.AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Session for Cart
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(1);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Email Service
builder.Services.AddTransient<IEmailService, EmailService>();

// -------------------- Build App -------------------- //

var app = builder.Build();

// -------------------- Seed Roles and Admin -------------------- //
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole<int>>>();
    var userManager = services.GetRequiredService<UserManager<User>>();

    await SeedRolesAndAdminUserAsync(roleManager, userManager);
}

async Task SeedRolesAndAdminUserAsync(RoleManager<IdentityRole<int>> roleManager, UserManager<User> userManager)
{
    string[] roles = { "Admin", "Customer", "Employee" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
    }

    string adminEmail = "admin@admin.com";
    string adminPassword = "Admin123!";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Name = "Admin"
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (result.Succeeded)
            await userManager.AddToRoleAsync(adminUser, "Admin");
    }
}

// -------------------- Middleware -------------------- //

// Error handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// IMPORTANT: Session before Auth
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// -------------------- Routes -------------------- //
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
