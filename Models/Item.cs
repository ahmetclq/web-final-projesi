using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwapSmart.Models;

public class Item
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Kullanıcı ID zorunludur.")]
    [Display(Name = "Sahip Kullanıcı")]
    public string OwnerUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ürün adı zorunludur.")]
    [MaxLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Ürün Adı")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Açıklama zorunludur.")]
    [MaxLength(2000, ErrorMessage = "Açıklama en fazla 2000 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Minimum değer zorunludur.")]
    [Range(0, int.MaxValue, ErrorMessage = "Minimum değer 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Minimum Değer (₺)")]
    public int EstimatedMinValue { get; set; }

    [Required(ErrorMessage = "Maximum değer zorunludur.")]
    [Range(0, int.MaxValue, ErrorMessage = "Maximum değer 0 veya daha büyük olmalıdır.")]
    [Display(Name = "Maximum Değer (₺)")]
    public int EstimatedMaxValue { get; set; }

    [Required(ErrorMessage = "Takas etmek istediğiniz ürün adı zorunludur.")]
    [MaxLength(200, ErrorMessage = "Ürün adı en fazla 200 karakter olabilir.")]
    [Display(Name = "Takas Etmek İstediğim Ürün")]
    public string WantedItemName { get; set; } = string.Empty;

    [Required(ErrorMessage = "İl zorunludur.")]
    [MaxLength(50, ErrorMessage = "İl adı en fazla 50 karakter olabilir.")]
    [Display(Name = "İl")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlçe zorunludur.")]
    [MaxLength(50, ErrorMessage = "İlçe adı en fazla 50 karakter olabilir.")]
    [Display(Name = "İlçe")]
    public string District { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Durum")]
    public ItemStatus Status { get; set; } = ItemStatus.Active;

    [Required]
    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("OwnerUserId")]
    public ApplicationUser? Owner { get; set; }

    public virtual ICollection<ItemImage> Images { get; set; } = new List<ItemImage>();
}

public enum ItemStatus
{
    Active = 1,
    InTrade = 2,
    Completed = 3
}
