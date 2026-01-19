using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SwapSmart.Models;

public class TradeOffer
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Teklif edilen ilan ID zorunludur.")]
    [Display(Name = "Teklif Edilen İlan")]
    public int OfferedItemId { get; set; }

    [Required(ErrorMessage = "Teklif verilen ilan ID zorunludur.")]
    [Display(Name = "Teklif Verilen İlan")]
    public int RequestedItemId { get; set; }

    [Required(ErrorMessage = "Gönderen kullanıcı ID zorunludur.")]
    [Display(Name = "Gönderen")]
    public string SenderUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Alıcı kullanıcı ID zorunludur.")]
    [Display(Name = "Alıcı")]
    public string ReceiverUserId { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Durum")]
    public TradeOfferStatus Status { get; set; } = TradeOfferStatus.Pending;

    [Display(Name = "İletişim Açıldı")]
    public bool IsContactOpened { get; set; } = false;

    [Required]
    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("OfferedItemId")]
    public virtual Item? OfferedItem { get; set; }

    [ForeignKey("RequestedItemId")]
    public virtual Item? RequestedItem { get; set; }

    [ForeignKey("SenderUserId")]
    public virtual ApplicationUser? Sender { get; set; }

    [ForeignKey("ReceiverUserId")]
    public virtual ApplicationUser? Receiver { get; set; }
}

public enum TradeOfferStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}
