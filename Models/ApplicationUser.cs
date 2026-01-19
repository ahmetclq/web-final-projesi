using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace SwapSmart.Models;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Ad Soyad zorunludur.")]
    [MaxLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [MaxLength(20, ErrorMessage = "Telefon numarası en fazla 20 karakter olabilir.")]
    [Display(Name = "Telefon Numarası")]
    public override string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "İl seçimi zorunludur.")]
    [MaxLength(50, ErrorMessage = "İl adı en fazla 50 karakter olabilir.")]
    [Display(Name = "İl")]
    public string City { get; set; } = string.Empty;

    [Required(ErrorMessage = "İlçe seçimi zorunludur.")]
    [MaxLength(50, ErrorMessage = "İlçe adı en fazla 50 karakter olabilir.")]
    [Display(Name = "İlçe")]
    public string District { get; set; } = string.Empty;
}
