using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwapSmart.Models;

public class ItemImage
{
    public int Id { get; set; }

    [Required(ErrorMessage = "İlan ID zorunludur.")]
    [Display(Name = "İlan")]
    public int ItemId { get; set; }

    [Required(ErrorMessage = "Fotoğraf yolu zorunludur.")]
    [MaxLength(500, ErrorMessage = "Fotoğraf yolu en fazla 500 karakter olabilir.")]
    [Display(Name = "Fotoğraf Yolu")]
    public string ImagePath { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey("ItemId")]
    public virtual Item? Item { get; set; }
}
