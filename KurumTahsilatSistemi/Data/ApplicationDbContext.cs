using KurumTahsilatSistemi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KurumTahsilatSistemi.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Kurum> Kurumlar { get; set; }

        public DbSet<TahsilatUrunu> TahsilatUrunleri { get; set; }

        public DbSet<Borc> Borclar { get; set; }

        public DbSet<Tahsilat> Tahsilatlar { get; set; }

        public DbSet<DosyaAktarim> DosyaAktarimlari { get; set; }

        public DbSet<Musteri> Musteriler { get; set; }

        public DbSet<AuditLogs> AuditLogs { get; set; }

        public DbSet<Bildirim> Bildirimler {  get; set; }


        protected override void OnModelCreating(
            ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Benzersiz alanlar
            modelBuilder.Entity<Kurum>()
                .HasIndex(x => x.KurumKodu)
                .IsUnique();

            modelBuilder.Entity<TahsilatUrunu>()
                .HasIndex(x => x.UrunKodu)
                .IsUnique();

            modelBuilder.Entity<Borc>()
                .HasIndex(x => x.ReferansNo)
                .IsUnique();

            modelBuilder.Entity<Kurum>()
                .HasIndex(x => x.HesapNo)
                .IsUnique()
                .HasFilter("[HesapNo] IS NOT NULL");

            // Kurum - Tahsilat Ürünü ilişkisi
            modelBuilder.Entity<TahsilatUrunu>()
                .HasOne(x => x.Kurum)
                .WithMany(x => x.TahsilatUrunleri)
                .HasForeignKey(x => x.KurumId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tahsilat Ürünü - Borç ilişkisi
            modelBuilder.Entity<Borc>()
                .HasOne(x => x.TahsilatUrunu)
                .WithMany(x => x.Borclar)
                .HasForeignKey(x => x.TahsilatUrunuId)
                .OnDelete(DeleteBehavior.Restrict);

            // Borç - Tahsilat ilişkisi
            modelBuilder.Entity<Tahsilat>()
                .HasOne(x => x.Borc)
                .WithMany(x => x.Tahsilatlar)
                .HasForeignKey(x => x.BorcId)
                .OnDelete(DeleteBehavior.Restrict);

            // Tahsilat - Identity Kullanıcısı ilişkisi
            modelBuilder.Entity<Tahsilat>()
                .HasOne(x => x.ApplicationUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Dosya Aktarımı - Kurum ilişkisi
            modelBuilder.Entity<DosyaAktarim>()
                .HasOne(x => x.Kurum)
                .WithMany()
                .HasForeignKey(x => x.KurumId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Musteri>()
                .HasIndex(x => x.MusteriNo)
                .IsUnique();

            modelBuilder.Entity<Musteri>()
                .HasIndex(x => x.Tckn)
                .IsUnique()
                .HasFilter("[Tckn] IS NOT NULL");

            modelBuilder.Entity<Musteri>()
              .HasIndex(x => x.VergiNo)
              .IsUnique()
              .HasFilter("[VergiNo] IS NOT NULL");

            modelBuilder.Entity<Borc>()
                .HasOne(x =>x.Musteri)
                .WithMany(x=>x.Borclar)
                .HasForeignKey(x =>x.MusteriId)
                .OnDelete(DeleteBehavior.Restrict); 


            modelBuilder.Entity<ApplicationUser>()
                .HasOne(x => x.Musteri)
                .WithMany()
                .HasForeignKey(x => x.MusteriId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasIndex(x => x.MusteriId)
                .IsUnique()
                .HasFilter("[MusteriId] IS NOT NULL");

            modelBuilder.Entity<Bildirim>()
                .HasOne(x => x.ApplicationUser)
                .WithMany(x => x.Bildirimler)
                .HasForeignKey(x => x.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Bildirim>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.OkunduMu,
                    x.OlusturmaTarihi
                });

            modelBuilder.Entity<Bildirim>()
                .HasOne(x => x.Borc)
                .WithMany()
                .HasForeignKey(x => x.BorcId)
                .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<Bildirim>()
                .HasIndex(x => new
                {
                    x.ApplicationUserId,
                    x.BildirimKodu
                })
                .IsUnique()
                .HasFilter("[BildirimKodu] IS NOT NULL");

            




        }
    }
}