using Microsoft.EntityFrameworkCore;
using SwapSmart.Data;
using SwapSmart.Models;

namespace SwapSmart.Services;

/// <summary>
/// Akıllı takas eşleştirme servisi.
/// Bir ilana uygun takas ilanlarını önerir.
/// </summary>
public class TradeMatchService
{
    private readonly ApplicationDbContext _context;

    public TradeMatchService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Bir ilan için uyumlu takas ilanlarını bulur ve önerir.
    /// </summary>
    /// <param name="item">Eşleştirme yapılacak ilan</param>
    /// <param name="maxResults">Maksimum öneri sayısı (varsayılan: 10)</param>
    /// <returns>Uyumlu ilanların listesi</returns>
    public async Task<List<Item>> FindMatchingItemsAsync(Item item, int maxResults = 10)
    {
        // Sadece Active durumundaki ilanları al
        var activeItems = await _context.Items
            .Include(i => i.Images)
            .Include(i => i.Owner)
            .Where(i => i.Status == ItemStatus.Active)
            .ToListAsync();

        var matches = new List<Item>();

        foreach (var otherItem in activeItems)
        {
            // Kendi ilanına öneri verme
            if (otherItem.OwnerUserId == item.OwnerUserId)
            {
                continue;
            }

            // Eşleştirme kontrolü
            if (IsMatch(item, otherItem))
            {
                matches.Add(otherItem);
            }
        }

        // En uyumlu olanları seç (şimdilik tüm eşleşenleri döndürüyoruz)
        // İleride skor sistemi eklenebilir
        return matches
            .OrderByDescending(m => m.CreatedAt)
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// İki ilanın birbiriyle eşleşip eşleşmediğini kontrol eder.
    /// </summary>
    private bool IsMatch(Item item, Item otherItem)
    {
        // Kural 1: Ürün - istek uyumu
        // item'in istediği ürün, otherItem'in adında geçiyor mu?
        var itemWantedMatch = ContainsIgnoreCase(otherItem.Title, item.WantedItemName);
        
        // otherItem'in istediği ürün, item'in adında geçiyor mu?
        var otherItemWantedMatch = ContainsIgnoreCase(item.Title, otherItem.WantedItemName);

        if (!itemWantedMatch && !otherItemWantedMatch)
        {
            return false; // Hiçbir uyum yok
        }

        // Kural 2: Değer aralığı uyumu
        // İki değer aralığı birbirini kesiyor mu?
        var valueMatch = item.EstimatedMinValue <= otherItem.EstimatedMaxValue &&
                        item.EstimatedMaxValue >= otherItem.EstimatedMinValue;

        if (!valueMatch)
        {
            return false; // Değer aralıkları uymuyor
        }

        // Kural 3: Konum uyumu (aynı il)
        var locationMatch = item.City == otherItem.City;

        if (!locationMatch)
        {
            return false; // Farklı şehirlerde
        }

        // Tüm kurallar sağlandıysa eşleşme var
        return true;
    }

    /// <summary>
    /// Case-insensitive string içerme kontrolü.
    /// </summary>
    private bool ContainsIgnoreCase(string source, string value)
    {
        if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(value))
        {
            return false;
        }

        return source.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
