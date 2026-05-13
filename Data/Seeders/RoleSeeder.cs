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
                string[] roleNames = { "SuperAdmin", "Admin", "Guest" };
                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        await roleManager.CreateAsync(new IdentityRole<int> { Name = roleName });
                        Console.WriteLine($"✅ {roleName} rolü oluşturuldu");
                    }
                }

                var superAdminEmail = "admin@stayhub.com";
                var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);

                if (superAdminUser == null)
                {
                    var superAdmin = new Guest
                    {
                        UserName = superAdminEmail,
                        Email = superAdminEmail,
                        FirstName = "StayHub",
                        LastName = "SuperAdmin",
                        PhoneNumber = "5550000000",
                        Country = "Turkey",
                        Address = "Merkez Ofis",
                        IdentificationNumber = "00000000000", 
                        DateOfBirth = new DateTime(1985, 1, 1),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(superAdmin, "Admin*123456");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                        await userManager.AddToRoleAsync(superAdmin, "Admin"); 
                        Console.WriteLine($"🚀 ANA SUPER ADMIN OLUŞTURULDU: {superAdminEmail}");
                    }
                }

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
                        Address = "Yönetim Paneli",
                        IdentificationNumber = "11111111111",
                        DateOfBirth = new DateTime(1990, 5, 20),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(manager, "Manager*123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(manager, "Admin");
                        Console.WriteLine($"✅ Örnek Admin oluşturuldu: {managerEmail}");
                    }
                }

                var guestUserEmail = "guest@stayhub.com";
                var guestUser = await userManager.FindByEmailAsync(guestUserEmail);

                if (guestUser == null)
                {
                    var guest = new Guest
                    {
                        UserName = guestUserEmail,
                        Email = guestUserEmail,
                        FirstName = "Test",
                        LastName = "Müşteri",
                        PhoneNumber = "5552222222",
                        Country = "Turkey",
                        Address = "Müşteri Mah.",
                        IdentificationNumber = "22222222222",
                        DateOfBirth = new DateTime(1998, 10, 10),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(guest, "Guest*123");
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