using Core.Concretes.Entities;
using Data.Contexts;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
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
                // ADMIN ROLE OLUSTUR
                var adminRoleExists = await roleManager.RoleExistsAsync("Admin");
                if (!adminRoleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = "Admin" });
                    Console.WriteLine("✅ Admin role oluşturuldu");
                }

                // GUEST ROLE OLUSTUR
                var guestRoleExists = await roleManager.RoleExistsAsync("Guest");
                if (!guestRoleExists)
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = "Guest" });
                    Console.WriteLine("✅ Guest role oluşturuldu");
                }

                // ADMIN USER OLUSTUR
                var adminUserEmail = "admin@stayhub.com";
                var adminUserExists = await userManager.FindByEmailAsync(adminUserEmail);

                if (adminUserExists == null)
                {
                    var adminUser = new Guest
                    {
                        UserName = adminUserEmail,
                        Email = adminUserEmail,
                        FirstName = "Admin",
                        LastName = "User",
                        PhoneNumber = "5551111111",
                        Country = "Turkey",
                        Address = "Admin Address",
                        IdentificationNumber = "12345678901",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(adminUser, "Admin@123");

                    if (result.Succeeded)
                    {
                        // Admin user'ı Admin role'üne ekle
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                        Console.WriteLine($"✅ Admin user oluşturuldu: {adminUserEmail}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Admin user oluşturulamadı: {string.Join(", ", result.Errors)}");
                    }
                }
                else
                {
                    // Zaten var, admin role'üne ekle
                    var isInAdminRole = await userManager.IsInRoleAsync(adminUserExists, "Admin");
                    if (!isInAdminRole)
                    {
                        await userManager.AddToRoleAsync(adminUserExists, "Admin");
                        Console.WriteLine($"✅ Admin user zaten vardı, Admin role'üne eklendi");
                    }
                }

                // TEST GUEST USER OLUSTUR
                var guestUserEmail = "guest@stayhub.com";
                var guestUserExists = await userManager.FindByEmailAsync(guestUserEmail);

                if (guestUserExists == null)
                {
                    var guestUser = new Guest
                    {
                        UserName = guestUserEmail,
                        Email = guestUserEmail,
                        FirstName = "Test",
                        LastName = "Guest",
                        PhoneNumber = "5552222222",
                        Country = "Turkey",
                        Address = "Guest Address",
                        IdentificationNumber = "10987654321",
                        DateOfBirth = new DateTime(1995, 6, 15),
                        IsActive = true,
                        EmailConfirmed = true,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await userManager.CreateAsync(guestUser, "Guest@123");

                    if (result.Succeeded)
                    {
                        // Guest user'ı Guest role'üne ekle
                        await userManager.AddToRoleAsync(guestUser, "Guest");
                        Console.WriteLine($"✅ Guest user oluşturuldu: {guestUserEmail}");
                    }
                    else
                    {
                        Console.WriteLine($"❌ Guest user oluşturulamadı: {string.Join(", ", result.Errors)}");
                    }
                }
                else
                {
                    // Zaten var, guest role'üne ekle
                    var isInGuestRole = await userManager.IsInRoleAsync(guestUserExists, "Guest");
                    if (!isInGuestRole)
                    {
                        await userManager.AddToRoleAsync(guestUserExists, "Guest");
                        Console.WriteLine($"✅ Guest user zaten vardı, Guest role'üne eklendi");
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