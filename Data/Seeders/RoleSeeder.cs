using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Seeders
{
    public class RoleSeeder
    {
        public static async Task SeedRolesAsync(
            UserManager<Guest> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            StayHubContext context)
        {
            try
            {
                // 1. Rolleri Oluştur
                string[] roleNames = { "SuperAdmin", "Admin", "Guest" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                        Console.WriteLine($"✅ {roleName} rolü oluşturuldu");
                    }
                }

                // 2. YENİ ANA SUPER ADMIN TANIMLAMASI
                var superAdminEmail = "superadmin@stayhub.com"; // Yeni email adresiniz
                var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

                if (superAdminUser == null)
                {
                    var superAdmin = new Guest
                    {
                        UserName = superAdminEmail,
                        Email = superAdminEmail,
                        FirstName = "StayHub",
                        LastName = "Yönetici",
                        PhoneNumber = "5550000000",
                        Country = "Turkey",
                        Address = "StayHub Ana Merkez",
                        IdentificationNumber = "99999999999",
                        DateOfBirth = new DateTime(1985, 1, 1),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    // Yeni Şifre: Admin123!
                    var result = await userManager.CreateAsync(superAdmin, "Admin123!");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                        await userManager.AddToRoleAsync(superAdmin, "Admin");
                        Console.WriteLine($"🚀 YENİ SUPER ADMIN OLUŞTURULDU: {superAdminEmail} / Şifre: Admin123!");
                    }
                }

                // 3. ÖRNEK OTEL YÖNETİCİSİ (ADMIN)
                var managerEmail = "manager@stayhub.com";
                if (await userManager.FindByEmailAsync(managerEmail) == null)
                {
                    var manager = new Guest
                    {
                        UserName = managerEmail,
                        Email = managerEmail,
                        FirstName = "Otel",
                        LastName = "Yöneticisi",
                        PhoneNumber = "5551111111",
                        Country = "Turkey",
                        Address = "Otel Yönetim Birimi",
                        IdentificationNumber = "11111111111",
                        DateOfBirth = new DateTime(1990, 5, 20),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    var result = await userManager.CreateAsync(manager, "Manager123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(manager, "Admin");
                        Console.WriteLine($"✅ Örnek Admin oluşturuldu: {managerEmail}");
                    }
                }

                // 4. ÖRNEK MÜŞTERİ (GUEST)
                var guestUserEmail = "guest@stayhub.com";
                if (await userManager.FindByEmailAsync(guestUserEmail) == null)
                {
                    var guest = new Guest
                    {
                        UserName = guestUserEmail,
                        Email = guestUserEmail,
                        FirstName = "Test",
                        LastName = "Müşteri",
                        PhoneNumber = "5552222222",
                        Country = "Turkey",
                        Address = "Örnek Mahalle no:1",
                        IdentificationNumber = "22222222222",
                        DateOfBirth = new DateTime(1998, 10, 10),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow,
                        IsDeleted = false
                    };

                    var result = await userManager.CreateAsync(guest, "Guest123!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(guest, "Guest");
                        Console.WriteLine($"✅ Test Müşterisi oluşturuldu: {guestUserEmail}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Seeder hatası: {ex.Message}");
                throw;
            }
        }
    }
}