using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwapSmart.Data;
using SwapSmart.Models;
using SwapSmart.Models.ViewModels;
using SwapSmart.Services;

namespace SwapSmart.Controllers;

[Authorize]
public class ItemController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TradeMatchService _matchService;

    public ItemController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        TradeMatchService matchService)
    {
        _context = context;
        _userManager = userManager;
        _matchService = matchService;
    }

    // GET: Item/MyItems
    public async Task<IActionResult> MyItems()
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
        {
            return NotFound();
        }

        var items = await _context.Items
            .Include(i => i.Images)
            .Where(i => i.OwnerUserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();

        return View(items);
    }

    // GET: Item/Create
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Kullanıcının il/ilçe bilgisi profilinden otomatik gelecek
        ViewBag.City = user.City;
        ViewBag.District = user.District;

        return View();
    }

    // POST: Item/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateItemViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        // Minimum <= Maximum kontrolü
        if (model.EstimatedMinValue > model.EstimatedMaxValue)
        {
            ModelState.AddModelError("EstimatedMaxValue", "Maximum değer, minimum değerden küçük olamaz.");
        }

        // En az 1 fotoğraf kontrolü
        if (model.Images == null || model.Images.Count == 0)
        {
            ModelState.AddModelError("Images", "En az bir fotoğraf yüklemelisiniz.");
        }

        if (ModelState.IsValid)
        {
            var item = new Item
            {
                OwnerUserId = user.Id,
                Title = model.Title,
                Description = model.Description,
                EstimatedMinValue = model.EstimatedMinValue,
                EstimatedMaxValue = model.EstimatedMaxValue,
                WantedItemName = model.WantedItemName,
                City = user.City,
                District = user.District,
                Status = ItemStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            // Fotoğrafları yükle
            if (model.Images != null && model.Images.Count > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "items");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                foreach (var imageFile in model.Images)
                {
                    if (imageFile.Length > 0)
                    {
                        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }

                        var imagePath = $"/uploads/items/{uniqueFileName}";
                        var itemImage = new ItemImage
                        {
                            ItemId = item.Id,
                            ImagePath = imagePath
                        };

                        _context.ItemImages.Add(itemImage);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(MyItems));
        }

        ViewBag.City = user.City;
        ViewBag.District = user.District;

        return View(model);
    }

    // GET: Item/Detail/5
    [AllowAnonymous]
    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var item = await _context.Items
            .Include(i => i.Images)
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id && i.Status == ItemStatus.Active);

        if (item == null)
        {
            return NotFound();
        }

        // Kullanıcı giriş yaptıysa, ilan sahibi mi kontrol et
        var currentUserId = _userManager.GetUserId(User);
        ViewBag.IsOwner = currentUserId != null && item.OwnerUserId == currentUserId;
        ViewBag.CurrentUserId = currentUserId;

        // Akıllı eşleştirme: Bu ilanla uyumlu takaslar
        if (item.Status == ItemStatus.Active)
        {
            var matchingItems = await _matchService.FindMatchingItemsAsync(item, maxResults: 4);
            ViewBag.MatchingItems = matchingItems;
        }
        else
        {
            ViewBag.MatchingItems = new List<Item>();
        }

        return View(item);
    }

    // GET: Item/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var item = await _context.Items
            .Include(i => i.Images)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);
        
        // İş Kuralı 1: Sadece kendi ilanını düzenleyebilir
        if (item.OwnerUserId != currentUserId)
        {
            TempData["Error"] = "Bu ilanı düzenleme yetkiniz yok.";
            return RedirectToAction(nameof(MyItems));
        }

        // İş Kuralı 2: Sadece Active durumundaki ilanlar düzenlenebilir
        if (item.Status != ItemStatus.Active)
        {
            TempData["Error"] = "Bu ilan güncellenemez. Sadece aktif ilanlar güncellenebilir.";
            return RedirectToAction(nameof(MyItems));
        }

        var viewModel = new EditItemViewModel
        {
            Id = item.Id,
            Title = item.Title,
            Description = item.Description,
            EstimatedMinValue = item.EstimatedMinValue,
            EstimatedMaxValue = item.EstimatedMaxValue,
            WantedItemName = item.WantedItemName,
            City = item.City,
            District = item.District,
            Status = item.Status
        };

        return View(viewModel);
    }

    // POST: Item/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditItemViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        var item = await _context.Items.FindAsync(id);
        if (item == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // İş Kuralı 1: Sadece kendi ilanını düzenleyebilir
        if (item.OwnerUserId != currentUserId)
        {
            TempData["Error"] = "Bu ilanı düzenleme yetkiniz yok.";
            return RedirectToAction(nameof(MyItems));
        }

        // İş Kuralı 2: Sadece Active durumundaki ilanlar düzenlenebilir
        if (item.Status != ItemStatus.Active)
        {
            TempData["Error"] = "Bu ilan güncellenemez. Sadece aktif ilanlar güncellenebilir.";
            return RedirectToAction(nameof(MyItems));
        }

        // Minimum <= Maximum kontrolü
        if (viewModel.EstimatedMinValue > viewModel.EstimatedMaxValue)
        {
            ModelState.AddModelError("EstimatedMaxValue", "Maximum değer, minimum değerden küçük olamaz.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Sadece belirtilen alanlar güncellenir
                item.Title = viewModel.Title;
                item.Description = viewModel.Description;
                item.EstimatedMinValue = viewModel.EstimatedMinValue;
                item.EstimatedMaxValue = viewModel.EstimatedMaxValue;
                item.WantedItemName = viewModel.WantedItemName;
                // City, District, OwnerUserId, Status DEĞİŞTİRİLEMEZ

                _context.Update(item);
                await _context.SaveChangesAsync();

                TempData["Success"] = "İlan başarıyla güncellendi.";
                return RedirectToAction(nameof(MyItems));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ItemExists(item.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }

        // Hata durumunda view model'i doldur
        viewModel.City = item.City;
        viewModel.District = item.District;
        viewModel.Status = item.Status;

        return View(viewModel);
    }

    // POST: Item/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.Items
            .Include(i => i.Images)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item == null)
        {
            TempData["Error"] = "İlan bulunamadı.";
            return RedirectToAction(nameof(MyItems));
        }

        var currentUserId = _userManager.GetUserId(User);

        // İş Kuralı 1: Sadece kendi ilanını silebilir
        if (item.OwnerUserId != currentUserId)
        {
            TempData["Error"] = "Bu ilanı silme yetkiniz yok.";
            return RedirectToAction(nameof(MyItems));
        }

        // İş Kuralı 2: Sadece Active durumundaki ilanlar silinebilir
        if (item.Status != ItemStatus.Active)
        {
            TempData["Error"] = "Bu ilan silinemez. Sadece aktif ilanlar silinebilir.";
            return RedirectToAction(nameof(MyItems));
        }

        // İleride TradeOffer tablosu eklendiğinde kontrol edilecek
        // Şimdilik bu kontrolü atlıyoruz çünkü TradeOffer tablosu yok

        try
        {
            // Fotoğrafları dosya sisteminden sil
            foreach (var image in item.Images)
            {
                var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // İlanı ve bağlı resimleri veritabanından sil
            _context.ItemImages.RemoveRange(item.Images);
            _context.Items.Remove(item);
            await _context.SaveChangesAsync();

            TempData["Success"] = "İlan başarıyla silindi.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = "İlan silinirken bir hata oluştu: " + ex.Message;
        }

        return RedirectToAction(nameof(MyItems));
    }

    private bool ItemExists(int id)
    {
        return _context.Items.Any(e => e.Id == id);
    }
}
