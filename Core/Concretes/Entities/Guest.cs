using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Core.Concretes.Entities
{
    /// <summary>
    /// Guest Entity - User identity işlemleri için
    /// IdentityUser<int>'den inherit ediyor
    /// </summary>
    public class Guest : IdentityUser<int>
    {
      
        public string FirstName { get; set; } = null!;

       
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Kimlik Numarası (11 haneli)
        /// </summary>
        public string? IdentificationNumber { get; set; }

        /// <summary>
        /// Doğum Tarihi
        /// </summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>
        /// Ülke
        /// </summary>
        public string Country { get; set; } = null!;

        /// <summary>
        /// Adres
        /// </summary>
        public string Address { get; set; } = null!;

        /// <summary>
        /// Aktif mi?
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Email doğrulanma tarihi
        /// </summary>
        public DateTime? EmailVerifiedAt { get; set; }

        /// <summary>
        /// Oluşturulma Tarihi
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Güncellenme Tarihi
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Son Giriş Tarihi
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Silinmiş mi?
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        public int? HotelId { get; set; } 
        public virtual Hotel? Hotel { get; set; }
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}