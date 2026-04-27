using System;
using System.Collections.Generic;

namespace Core.Concretes.DTOs
{
    /// <summary>
    /// Otel filtreleme için DTO
    /// </summary>
    public class HotelFilterDto
    {
        /// <summary>
        /// Otel adı (contains arama)
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Şehir
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Bölge
        /// </summary>
        public string? Region { get; set; }

        /// <summary>
        /// Ülke
        /// </summary>
        public string? Country { get; set; }

        /// <summary>
        /// Minimum yıldız puanı (1-5)
        /// </summary>
        public decimal? MinRating { get; set; }

        /// <summary>
        /// Maksimum yıldız puanı (1-5)
        /// </summary>
        public decimal? MaxRating { get; set; }

        /// <summary>
        /// Minimum fiyat
        /// </summary>
        public decimal? MinPrice { get; set; }

        /// <summary>
        /// Maksimum fiyat
        /// </summary>
        public decimal? MaxPrice { get; set; }

        /// <summary>
        /// Check-in tarihi
        /// </summary>
        public DateTime? CheckInDate { get; set; }

        /// <summary>
        /// Check-out tarihi
        /// </summary>
        public DateTime? CheckOutDate { get; set; }

        /// <summary>
        /// Sadece aktif otelleri getir
        /// </summary>
        public bool? IsActive { get; set; } = true;

        /// <summary>
        /// Sıralama (Name, Rating, Price)
        /// </summary>
        public string? SortBy { get; set; } = "Name";

        /// <summary>
        /// Sıralama yönü (asc, desc)
        /// </summary>
        public string? SortOrder { get; set; } = "asc";

        /// <summary>
        /// Sayfa numarası
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Sayfa boyutu
        /// </summary>
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Otel türü (Luxury, Budget, Business, vb.)
        /// </summary>
        public string? HotelType { get; set; }

        /// <summary>
        /// Minimum oda sayısı
        /// </summary>
        public int? MinRoomCount { get; set; }

        /// <summary>
        /// Özel olanaklar (WiFi, Pool, vb.) - virgülle ayırılmış
        /// </summary>
        public string? Amenities { get; set; }
    }
}