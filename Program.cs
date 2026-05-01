using Microsoft.EntityFrameworkCore;
using OnlineQuizSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// ── 1. Database Connection ──────────────────────────────
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── 2. Cookie Authentication ────────────────────────────
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "QuizSystemCookie";
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

// ── 3. MVC ──────────────────────────────────────────────
builder.Services.AddControllersWithViews();

var app = builder.Build();

// ── 4. Middleware Pipeline ──────────────────────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();   // Pehle Authentication
app.UseAuthorization();    // Phir Authorization

// ── 5. Default Route ────────────────────────────────────
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();