using System.Linq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SwapSmart.Data;
using SwapSmart.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Services
builder.Services.AddScoped<SwapSmart.Services.TradeMatchService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
// Auto-apply migrations on startup - daha erken çalıştır
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Veritabanı var mı kontrol et
        if (!context.Database.CanConnect())
        {
            logger.LogInformation("Veritabanı bulunamadı, oluşturuluyor...");
        }
        
        // Tüm migration'ları uygula
        var pendingMigrations = context.Database.GetPendingMigrations().ToList();
        if (pendingMigrations.Any())
        {
            logger.LogInformation($"{pendingMigrations.Count} migration uygulanıyor: {string.Join(", ", pendingMigrations)}");
            context.Database.Migrate();
            logger.LogInformation("Migration'lar başarıyla uygulandı.");
        }
        else
        {
            logger.LogInformation("Uygulanacak migration yok.");
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Migration hatası: {Message}", ex.Message);
        
        // Geliştirme ortamında EnsureCreated kullan (fallback)
        if (app.Environment.IsDevelopment())
        {
            try
            {
                logger.LogWarning("EnsureCreated() ile veritabanı oluşturulmaya çalışılıyor...");
                var context = services.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();
                logger.LogInformation("Veritabanı EnsureCreated() ile oluşturuldu.");
            }
            catch (Exception ensureEx)
            {
                logger.LogError(ensureEx, "EnsureCreated() de başarısız oldu.");
            }
        }
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
