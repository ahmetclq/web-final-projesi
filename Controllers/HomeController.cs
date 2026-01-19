using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwapSmart.Data;
using SwapSmart.Models;

namespace SwapSmart.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public HomeController(
        ILogger<HomeController> logger,
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? district = null)
    {
        var itemsQuery = _context.Items
            .Include(i => i.Images)
            .Include(i => i.Owner)
            .Where(i => i.Status == ItemStatus.Active);

        // Giriş yapan kullanıcı varsa, aynı ildeki ilanları göster
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                itemsQuery = itemsQuery.Where(i => i.City == user.City);

                // İlçe filtresi varsa uygula
                if (!string.IsNullOrEmpty(district))
                {
                    itemsQuery = itemsQuery.Where(i => i.District == district);
                }

                ViewBag.UserCity = user.City;
                ViewBag.UserDistrict = user.District;

                // Aynı ildeki farklı ilçeleri al (filtre için)
                var districts = await _context.Items
                    .Where(i => i.City == user.City && i.Status == ItemStatus.Active)
                    .Select(i => i.District)
                    .Distinct()
                    .OrderBy(d => d)
                    .ToListAsync();

                ViewBag.AvailableDistricts = districts;
                ViewBag.SelectedDistrict = district;
            }
        }
        else
        {
            // Giriş yapmayan kullanıcılar için tüm ilanları göster (veya boş liste)
            itemsQuery = itemsQuery.Where(i => false); // Şimdilik giriş yapmayanlar hiçbir şey görmesin
        }

        var items = await itemsQuery
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
