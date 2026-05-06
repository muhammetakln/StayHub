using Core.Concretes.Entities;
using Core.Concretes.Enum;
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
                    HotelType = "Luxury",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    // 👇 OTELLERİN İÇİNE ÖRNEK ODALAR EKLİYORUZ 👇
                    Rooms = new List<Room>
                    {
                        new Room { RoomNumber = "101", Name = "Standart Deniz Manzaralı", Type = RoomType.Double, Capacity = 2, PricePerNight = 2500, Price = 2500, Size = 30, FloorNumber = 1, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Room { RoomNumber = "102", Name = "Kral Dairesi", Type = RoomType.Suite, Capacity = 4, PricePerNight = 7500, Price = 7500, Size = 80, FloorNumber = 1, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow }
                    }
                },
                new Hotel
                {
                    Name = "Marriott Ankara",
                    City = "Ankara",
                    Region = "Cankaya",
                    Country = "Turkey",
                    Address = "Cankaya, Ankara",
                    PhoneNumber = "5559876543",
                    Email = "marriott@ankara.com",
                    Website = "https://marriott.com",
                    Description = "Business hotel in Ankara for corporate travelers",
                    HotelType = "Business",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    Rooms = new List<Room>
                    {
                        new Room { RoomNumber = "201", Name = "Business Suit", Type = RoomType.Suite, Capacity = 2, PricePerNight = 1800, Price = 1800, Size = 45, FloorNumber = 2, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow },
                        new Room { RoomNumber = "202", Name = "Standart Şehir Manzaralı", Type = RoomType.Single, Capacity = 1, PricePerNight = 1200, Price = 1200, Size = 25, FloorNumber = 2, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow }
                    }
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
                    HotelType = "Budget",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    Rooms = new List<Room>
                    {
                        new Room { RoomNumber = "301", Name = "Ekonomik Oda", Type = RoomType.Double, Capacity = 2, PricePerNight = 800, Price = 800, Size = 20, FloorNumber = 3, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow }
                    }
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
                    HotelType = "Resort",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    Rooms = new List<Room>
                    {
                        new Room { RoomNumber = "401", Name = "Aile Odası (Havuz Manzaralı)", Type = RoomType.Twin, Capacity = 4, PricePerNight = 3500, Price = 3500, Size = 50, FloorNumber = 4, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow }
                    }
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
                    HotelType = "Boutique",
                    IsActive = true,
                    IsDeleted = false,
                    CreatedAt = DateTime.UtcNow,
                    Rooms = new List<Room>
                    {
                        new Room { RoomNumber = "501", Name = "Taş Oda (Cave Room)", Type = RoomType.Double, Capacity = 2, PricePerNight = 4000, Price = 4000, Size = 35, FloorNumber = 5, Status = RoomStatus.Available, IsActive = true, CreatedAt = DateTime.UtcNow }
                    }
                }
            };

            context.Hotels.AddRange(hotels);
            context.SaveChanges();
            Console.WriteLine($"[Seeder] {hotels.Count} otel ve ilgili odalar oluşturuldu.");
        }
    }
}