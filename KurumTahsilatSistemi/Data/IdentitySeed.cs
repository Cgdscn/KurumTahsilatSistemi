using KurumTahsilatSistemi.Models;
using Microsoft.AspNetCore.Identity;

namespace KurumTahsilatSistemi.Data
{
    public static class IdentitySeed
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider)
        {
            var roleManager =
                serviceProvider.GetRequiredService<
                    RoleManager<IdentityRole>>();

            var userManager =
                serviceProvider.GetRequiredService<
                    UserManager<ApplicationUser>>();

            string[] roller =
            {
                "Admin",
                "Kullanici"
            };

            foreach (var rol in roller)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                {
                    var rolSonucu =
                        await roleManager.CreateAsync(
                            new IdentityRole(rol));

                    if (!rolSonucu.Succeeded)
                    {
                        var hatalar = string.Join(
                            ", ",
                            rolSonucu.Errors.Select(x => x.Description));

                        throw new Exception(
                            $"'{rol}' rolü oluşturulamadı: {hatalar}");
                    }
                }
            }

            await KullaniciOlusturAsync(
                userManager,
                email: "admin@kurumtahsilat.local",
                parola: "Admin123!",
                adSoyad: "Sistem Yöneticisi",
                rol: "Admin");

            await KullaniciOlusturAsync(
                userManager,
                email: "kullanici@kurumtahsilat.local",
                parola: "Kullanici123!",
                adSoyad: "Tahsilat Kullanıcısı",
                rol: "Kullanici");
        }

        private static async Task KullaniciOlusturAsync(
            UserManager<ApplicationUser> userManager,
            string email,
            string parola,
            string adSoyad,
            string rol)
        {
            var kullanici =
                await userManager.FindByEmailAsync(email);

            if (kullanici == null)
            {
                kullanici = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    AdSoyad = adSoyad,
                    AktifMi = true,
                    OlusturmaTarihi = DateTime.Now
                };

                var kullaniciSonucu =
                    await userManager.CreateAsync(
                        kullanici,
                        parola);

                if (!kullaniciSonucu.Succeeded)
                {
                    var hatalar = string.Join(
                        ", ",
                        kullaniciSonucu.Errors
                            .Select(x => x.Description));

                    throw new Exception(
                        $"'{email}' kullanıcısı oluşturulamadı: {hatalar}");
                }
            }

            if (!await userManager.IsInRoleAsync(kullanici, rol))
            {
                var rolSonucu =
                    await userManager.AddToRoleAsync(
                        kullanici,
                        rol);

                if (!rolSonucu.Succeeded)
                {
                    var hatalar = string.Join(
                        ", ",
                        rolSonucu.Errors
                            .Select(x => x.Description));

                    throw new Exception(
                        $"Kullanıcıya rol atanamadı: {hatalar}");
                }
            }
        }
    }
}