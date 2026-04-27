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
        // ═══════════════════════════════════════════════════════════════
        // Inherited from IdentityUser<int>:
        // ═══════════════════════════════════════════════════════════════
        // public int Id { get; set; }
        // public string UserName { get; set; }
        // public string NormalizedUserName { get; set; }
        // public string Email { get; set; }
        // public string NormalizedEmail { get; set; }
        // public bool EmailConfirmed { get; set; }
        // public string PasswordHash { get; set; }
        // public string SecurityStamp { get; set; }
        // public string ConcurrencyStamp { get; set; }
        // public string PhoneNumber { get; set; }
        // public bool PhoneNumberConfirmed { get; set; }
        // public bool TwoFactorEnabled { get; set; }
        // public DateTimeOffset? LockoutEnd { get; set; }
        // public bool LockoutEnabled { get; set; }
        // public int AccessFailedCount { get; set; }

        // ═══════════════════════════════════════════════════════════════
        // Custom Properties (Required!)
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Adı
        /// </summary>
        public string FirstName { get; set; } = null!;

        /// <summary>
        /// Soyadı
        /// </summary>
        public string LastName { get; set; } = null!;

        /// <summary>
        /// Kimlik Numarası (11 haneli)
        /// </summary>
        public string IdentificationNumber { get; set; } = null!;

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

        // ═══════════════════════════════════════════════════════════════
        // Navigation Properties
        // ═══════════════════════════════════════════════════════════════

        /// <summary>
        /// Guest'in rezervasyonları
        /// </summary>
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

        /// <summary>
        /// Guest'in yorumları
        /// </summary>
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}