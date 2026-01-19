using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SwapSmart.Models;

namespace SwapSmart.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
    public DbSet<ItemImage> ItemImages { get; set; }
    public DbSet<TradeOffer> TradeOffers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Item yapılandırması
        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OwnerUserId);
            entity.HasOne(e => e.Owner)
                .WithMany()
                .HasForeignKey(e => e.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.Images)
                .WithOne(i => i.Item)
                .HasForeignKey(i => i.ItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ItemImage yapılandırması
        modelBuilder.Entity<ItemImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ItemId);
        });

        // TradeOffer yapılandırması
        modelBuilder.Entity<TradeOffer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OfferedItemId);
            entity.HasIndex(e => e.RequestedItemId);
            entity.HasIndex(e => e.SenderUserId);
            entity.HasIndex(e => e.ReceiverUserId);
            
            entity.HasOne(e => e.OfferedItem)
                .WithMany()
                .HasForeignKey(e => e.OfferedItemId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.RequestedItem)
                .WithMany()
                .HasForeignKey(e => e.RequestedItemId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Sender)
                .WithMany()
                .HasForeignKey(e => e.SenderUserId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Receiver)
                .WithMany()
                .HasForeignKey(e => e.ReceiverUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
