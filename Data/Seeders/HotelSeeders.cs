using Core.Concretes.Entities;
using Data.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Data.Seeders
{
    public class HotelSeeder
    {
        public static void SeedHotels(StayHubContext context)
        {
            // Zaten veri varsa ekleme yapma
            if (context.Hotels.Any())
                return;

            var hotels = new List<Hotel>
            {
                new Hotel
                {
                    Name = "Hilton Istanbul",
                    City = "Istanbul",
                    Region = "Besiktas",
                    Country = "Turkey",
                    Address = "Besiktas, Istanbul",
                    PhoneNumber = "5551234567",
                    Email = "hilton@istanbul.com",
                    Website = "https://hilton.com",
                    Description = "Luxury hotel in Istanbul with stunning views",
                    Rating = "4.5",
                    HotelType = "Luxury",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Name = "Marriott Ankara",
                    City = "Ankara",
                    Region = "Cankiri",
                    Country = "Turkey",
                    Address = "Cankiri, Ankara",
                    PhoneNumber = "5559876543",
                    Email = "marriott@ankara.com",
                    Website = "https://marriott.com",
                    Description = "Business hotel in Ankara for corporate travelers",
                    Rating = "4.0",
                    HotelType = "Business",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Name = "Budget Inn Izmir",
                    City = "Izmir",
                    Region = "Alsancak",
                    Country = "Turkey",
                    Address = "Alsancak, Izmir",
                    PhoneNumber = "5554443322",
                    Email = "budget@izmir.com",
                    Website = "https://budgetinn.com",
                    Description = "Affordable and comfortable budget hotel in Izmir",
                    Rating = "3.5",
                    HotelType = "Budget",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Name = "Antalya Resort",
                    City = "Antalya",
                    Region = "Konyaalti",
                    Country = "Turkey",
                    Address = "Konyaalti, Antalya",
                    PhoneNumber = "5558889999",
                    Email = "resort@antalya.com",
                    Website = "https://antalyaresort.com",
                    Description = "All-inclusive beach resort in Antalya",
                    Rating = "4.7",
                    HotelType = "Resort",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                },
                new Hotel
                {
                    Name = "Cappadocia Boutique",
                    City = "Nevsehir",
                    Region = "Cappadocia",
                    Country = "Turkey",
                    Address = "Cappadocia, Nevsehir",
                    PhoneNumber = "5557776666",
                    Email = "boutique@cappadocia.com",
                    Website = "https://cappadociaboutique.com",
                    Description = "Unique boutique hotel in historical Cappadocia",
                    Rating = "4.8",
                    HotelType = "Boutique",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow
                }
            };

            context.Hotels.AddRange(hotels);
            context.SaveChanges();

            Console.WriteLine($"✅ {hotels.Count} otel oluşturuldu");
        }
    }
}