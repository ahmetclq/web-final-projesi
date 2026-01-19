using System.ComponentModel.DataAnnotations;

namespace SwapSmart.Models.ViewModels;

public class EditItemViewModel
{
    public int Id { get; set; }

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

    // Read-only alanlar
    public string City { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;
    public ItemStatus Status { get; set; }
}
