using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SwapSmart.Data;
using SwapSmart.Models;
using SwapSmart.Services;

namespace SwapSmart.Controllers;

[Authorize]
public class TradeController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly TradeMatchService _matchService;

    public TradeController(
        ApplicationDbContext context, 
        UserManager<ApplicationUser> userManager,
        TradeMatchService matchService)
    {
        _context = context;
        _userManager = userManager;
        _matchService = matchService;
    }

    // GET: Trade/Send/5
    public async Task<IActionResult> Send(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var requestedItem = await _context.Items
            .Include(i => i.Images)
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (requestedItem == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // İş Kuralı 1: Kullanıcı kendi ilanına teklif gönderemez
        if (requestedItem.OwnerUserId == currentUserId)
        {
            TempData["Error"] = "Kendi ilanınıza teklif gönderemezsiniz.";
            return RedirectToAction("Detail", "Item", new { id = id });
        }

        // İş Kuralı 2: RequestedItem.Status == Active olmalı (Completed veya InTrade ilanlara teklif gönderilemez)
        if (requestedItem.Status != ItemStatus.Active)
        {
            TempData["Error"] = "Bu ilan aktif değil veya tamamlanmış, teklif gönderemezsiniz.";
            return RedirectToAction("Detail", "Item", new { id = id });
        }

        // Kullanıcının kendi Active ilanlarını getir
        var userItems = await _context.Items
            .Where(i => i.OwnerUserId == currentUserId && i.Status == ItemStatus.Active)
            .ToListAsync();

        if (!userItems.Any())
        {
            TempData["Error"] = "Teklif göndermek için en az bir aktif ilanınız olmalıdır.";
            return RedirectToAction("MyItems", "Item");
        }

        ViewBag.RequestedItem = requestedItem;
        ViewBag.UserItems = userItems;

        return View();
    }

    // POST: Trade/Send
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Send(int requestedItemId, int offeredItemId)
    {
        var currentUserId = _userManager.GetUserId(User);

        var requestedItem = await _context.Items
            .Include(i => i.Owner)
            .FirstOrDefaultAsync(i => i.Id == requestedItemId);

        var offeredItem = await _context.Items
            .FirstOrDefaultAsync(i => i.Id == offeredItemId);

        if (requestedItem == null || offeredItem == null)
        {
            return NotFound();
        }

        // İş Kuralı 1: Kullanıcı kendi ilanına teklif gönderemez
        if (requestedItem.OwnerUserId == currentUserId)
        {
            TempData["Error"] = "Kendi ilanınıza teklif gönderemezsiniz.";
            return RedirectToAction("Detail", "Item", new { id = requestedItemId });
        }

        // İş Kuralı 2: RequestedItem.Status == Active olmalı (Completed veya InTrade ilanlara teklif gönderilemez)
        if (requestedItem.Status != ItemStatus.Active)
        {
            TempData["Error"] = "Bu ilan aktif değil veya tamamlanmış, teklif gönderemezsiniz.";
            return RedirectToAction("Detail", "Item", new { id = requestedItemId });
        }

        // İş Kuralı 3: OfferedItem kullanıcıya ait ve Status == Active olmalı
        if (offeredItem.OwnerUserId != currentUserId || offeredItem.Status != ItemStatus.Active)
        {
            TempData["Error"] = "Seçtiğiniz ilan aktif değil veya size ait değil.";
            return RedirectToAction("Send", new { id = requestedItemId });
        }

        // İş Kuralı 4: Aynı iki ilan arasında birden fazla Pending teklif olamaz
        var existingPendingOffer = await _context.TradeOffers
            .FirstOrDefaultAsync(t => t.OfferedItemId == offeredItemId &&
                                     t.RequestedItemId == requestedItemId &&
                                     t.Status == TradeOfferStatus.Pending);

        if (existingPendingOffer != null)
        {
            TempData["Error"] = "Bu ilanlara zaten bekleyen bir teklif gönderdiniz.";
            return RedirectToAction("Outgoing");
        }

        var tradeOffer = new TradeOffer
        {
            OfferedItemId = offeredItemId,
            RequestedItemId = requestedItemId,
            SenderUserId = currentUserId,
            ReceiverUserId = requestedItem.OwnerUserId,
            Status = TradeOfferStatus.Pending,
            IsContactOpened = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.TradeOffers.Add(tradeOffer);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Takas teklifi başarıyla gönderildi.";
        return RedirectToAction(nameof(Outgoing));
    }

    // GET: Trade/Incoming
    public async Task<IActionResult> Incoming()
    {
        var currentUserId = _userManager.GetUserId(User);

        var incomingOffers = await _context.TradeOffers
            .Include(t => t.OfferedItem!)
                .ThenInclude(i => i.Images)
            .Include(t => t.RequestedItem!)
                .ThenInclude(i => i.Images)
            .Include(t => t.Sender)
            .Where(t => t.ReceiverUserId == currentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // WhatsApp linklerini dictionary olarak hazırla
        var whatsAppLinks = new Dictionary<int, string>();
        foreach (var offer in incomingOffers)
        {
            if (offer.Status == TradeOfferStatus.Accepted && offer.IsContactOpened)
            {
                var link = GenerateWhatsAppLink(offer.Sender?.PhoneNumber);
                if (!string.IsNullOrEmpty(link))
                {
                    whatsAppLinks[offer.Id] = link;
                }
            }
        }
        ViewBag.WhatsAppLinks = whatsAppLinks;

        return View(incomingOffers);
    }

    // GET: Trade/Outgoing
    public async Task<IActionResult> Outgoing()
    {
        var currentUserId = _userManager.GetUserId(User);

        var outgoingOffers = await _context.TradeOffers
            .Include(t => t.OfferedItem!)
                .ThenInclude(i => i.Images)
            .Include(t => t.RequestedItem!)
                .ThenInclude(i => i.Images)
            .Include(t => t.Receiver)
            .Where(t => t.SenderUserId == currentUserId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();

        // WhatsApp linklerini dictionary olarak hazırla
        var whatsAppLinks = new Dictionary<int, string>();
        foreach (var offer in outgoingOffers)
        {
            if (offer.Status == TradeOfferStatus.Accepted && offer.IsContactOpened)
            {
                var link = GenerateWhatsAppLink(offer.Receiver?.PhoneNumber);
                if (!string.IsNullOrEmpty(link))
                {
                    whatsAppLinks[offer.Id] = link;
                }
            }
        }
        ViewBag.WhatsAppLinks = whatsAppLinks;

        return View(outgoingOffers);
    }

    // POST: Trade/Accept/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Accept(int id)
    {
        var tradeOffer = await _context.TradeOffers
            .Include(t => t.OfferedItem)
            .Include(t => t.RequestedItem)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tradeOffer == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Güvenlik: Sadece ReceiverUserId kabul edebilir
        if (tradeOffer.ReceiverUserId != currentUserId)
        {
            TempData["Error"] = "Bu teklifi kabul etme yetkiniz yok.";
            return RedirectToAction(nameof(Incoming));
        }

        if (tradeOffer.Status != TradeOfferStatus.Pending)
        {
            TempData["Error"] = "Sadece bekleyen teklifler kabul edilebilir.";
            return RedirectToAction(nameof(Incoming));
        }

        // İş Kuralı: Kabul edilince diğer Pending teklifleri reddet
        var otherPendingOffers = await _context.TradeOffers
            .Where(t => t.RequestedItemId == tradeOffer.RequestedItemId &&
                       t.Id != id &&
                       t.Status == TradeOfferStatus.Pending)
            .ToListAsync();

        foreach (var offer in otherPendingOffers)
        {
            offer.Status = TradeOfferStatus.Rejected;
        }

        // Teklifi kabul et
        tradeOffer.Status = TradeOfferStatus.Accepted;
        tradeOffer.IsContactOpened = true; // WhatsApp iletişimi açık

        // İlan durumlarını güncelle
        if (tradeOffer.RequestedItem != null)
            tradeOffer.RequestedItem.Status = ItemStatus.InTrade;
        if (tradeOffer.OfferedItem != null)
            tradeOffer.OfferedItem.Status = ItemStatus.InTrade;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Takas teklifi kabul edildi. Her iki ilan da 'Takasta' durumuna geçti. Artık WhatsApp üzerinden iletişime geçebilirsiniz.";
        return RedirectToAction(nameof(Incoming));
    }

    // POST: Trade/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var tradeOffer = await _context.TradeOffers
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tradeOffer == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Güvenlik: Sadece ReceiverUserId reddedebilir
        if (tradeOffer.ReceiverUserId != currentUserId)
        {
            TempData["Error"] = "Bu teklifi reddetme yetkiniz yok.";
            return RedirectToAction(nameof(Incoming));
        }

        if (tradeOffer.Status != TradeOfferStatus.Pending)
        {
            TempData["Error"] = "Sadece bekleyen teklifler reddedilebilir.";
            return RedirectToAction(nameof(Incoming));
        }

        tradeOffer.Status = TradeOfferStatus.Rejected;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Takas teklifi reddedildi.";
        return RedirectToAction(nameof(Incoming));
    }

    // POST: Trade/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        var tradeOffer = await _context.TradeOffers
            .Include(t => t.OfferedItem)
            .Include(t => t.RequestedItem)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (tradeOffer == null)
        {
            return NotFound();
        }

        var currentUserId = _userManager.GetUserId(User);

        // Güvenlik: Sadece taraflar tamamlayabilir
        if (tradeOffer.SenderUserId != currentUserId && tradeOffer.ReceiverUserId != currentUserId)
        {
            TempData["Error"] = "Bu takası tamamlama yetkiniz yok.";
            return RedirectToAction(nameof(Incoming));
        }

        // Sadece Accepted teklifler tamamlanabilir
        if (tradeOffer.Status != TradeOfferStatus.Accepted)
        {
            TempData["Error"] = "Sadece kabul edilmiş takaslar tamamlanabilir.";
            return RedirectToAction(nameof(Incoming));
        }

        // İlan durumlarını Completed yap
        if (tradeOffer.RequestedItem != null)
            tradeOffer.RequestedItem.Status = ItemStatus.Completed;
        if (tradeOffer.OfferedItem != null)
            tradeOffer.OfferedItem.Status = ItemStatus.Completed;
        // TradeOffer.Status değişmez, Accepted kalır

        await _context.SaveChangesAsync();

        TempData["Success"] = "Takas başarıyla tamamlandı. İlanlar 'Tamamlandı' durumuna geçti.";
        
        // Kullanıcıya göre yönlendirme
        if (tradeOffer.ReceiverUserId == currentUserId)
        {
            return RedirectToAction(nameof(Incoming));
        }
        else
        {
            return RedirectToAction(nameof(Outgoing));
        }
    }

    // Helper method: WhatsApp link oluştur
    private string? GenerateWhatsAppLink(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber))
        {
            return null;
        }

        // Telefon numarasından özel karakterleri temizle
        var cleanPhone = phoneNumber.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "");
        
        // Başında +90 varsa kaldır, yoksa 90 ekle (Türkiye için)
        if (cleanPhone.StartsWith("+90"))
        {
            cleanPhone = cleanPhone.Substring(3);
        }
        else if (cleanPhone.StartsWith("90"))
        {
            // Zaten 90 var
        }
        else if (cleanPhone.StartsWith("0"))
        {
            cleanPhone = cleanPhone.Substring(1);
            cleanPhone = "90" + cleanPhone;
        }
        else
        {
            cleanPhone = "90" + cleanPhone;
        }

        var message = Uri.EscapeDataString("Takas teklifiniz kabul edildi. İletişime geçebiliriz.");
        return $"https://wa.me/{cleanPhone}?text={message}";
    }

    // GET: Trade/Recommended
    public async Task<IActionResult> Recommended()
    {
        var currentUserId = _userManager.GetUserId(User);
        if (currentUserId == null)
        {
            return NotFound();
        }

        // Kullanıcının Active ilanlarını al
        var userActiveItems = await _context.Items
            .Include(i => i.Images)
            .Where(i => i.OwnerUserId == currentUserId && i.Status == ItemStatus.Active)
            .ToListAsync();

        if (!userActiveItems.Any())
        {
            ViewBag.HasActiveItems = false;
            return View(new List<Item>());
        }

        // Her ilan için eşleşmeleri bul
        var allMatches = new List<Item>();
        var matchedItemIds = new HashSet<int>(); // Tekrar eden ilanları filtrelemek için

        foreach (var item in userActiveItems)
        {
            var matches = await _matchService.FindMatchingItemsAsync(item, maxResults: 20);
            
            foreach (var match in matches)
            {
                // Daha önce eklenmemişse ekle
                if (!matchedItemIds.Contains(match.Id))
                {
                    allMatches.Add(match);
                    matchedItemIds.Add(match.Id);
                }
            }
        }

        // En yeni olanları önce göster
        var recommendedItems = allMatches
            .OrderByDescending(i => i.CreatedAt)
            .ToList();

        ViewBag.HasActiveItems = true;
        return View(recommendedItems);
    }
}
