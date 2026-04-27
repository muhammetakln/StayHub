using System.ComponentModel.DataAnnotations;
public partial class RoleDto
{
    /// <summary>
    /// Permission DTO (List)
    /// </summary>
    public class PermissionDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "İzin Kodu")]
        public string Code { get; set; } = null!;

        [Display(Name = "Açıklama")]
        public string Description { get; set; } = null!;

        [Display(Name = "Kategori")]
        public string Category { get; set; } = null!;

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }


        /// <summary>
        /// Permission Detail DTO
        /// </summary>
        public class PermissionDetailDto
        {
            [Display(Name = "ID")]
            public int Id { get; set; }

            [Display(Name = "İzin Kodu")]
            public string Code { get; set; } = null!;

            [Display(Name = "Açıklama")]
            public string Description { get; set; } = null!;

            [Display(Name = "Kategori")]
            public string Category { get; set; } = null!;

            [Display(Name = "Aktif")]
            public bool IsActive { get; set; }

            [Display(Name = "Rol Sayısı")]
            public int RoleCount { get; set; }

            [Display(Name = "Oluşturulma Tarihi")]
            public DateTime CreatedAt { get; set; }
        }

        /// <summary>
        /// Create Permission DTO
        /// </summary>
        public class CreatePermissionDto
        {
            [Required(ErrorMessage = "İzin kodu gereklidir")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "İzin kodu 3-100 karakter arasında olmalıdır")]
            [Display(Name = "İzin Kodu")]
            public string Code { get; set; } = null!;

            [Required(ErrorMessage = "Açıklama gereklidir")]
            [StringLength(500, MinimumLength = 5, ErrorMessage = "Açıklama 5-500 karakter arasında olmalıdır")]
            [Display(Name = "Açıklama")]
            public string Description { get; set; } = null!;

            [Required(ErrorMessage = "Kategori gereklidir")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Kategori 3-50 karakter arasında olmalıdır")]
            [Display(Name = "Kategori")]
            public string Category { get; set; } = null!;
        }

        /// <summary>
        /// Update Permission DTO
        /// </summary>
        public class UpdatePermissionDto
        {
            [Required(ErrorMessage = "İzin kodu gereklidir")]
            [StringLength(100, MinimumLength = 3, ErrorMessage = "İzin kodu 3-100 karakter arasında olmalıdır")]
            [Display(Name = "İzin Kodu")]
            public string Code { get; set; } = null!;

            [Required(ErrorMessage = "Açıklama gereklidir")]
            [StringLength(500, MinimumLength = 5, ErrorMessage = "Açıklama 5-500 karakter arasında olmalıdır")]
            [Display(Name = "Açıklama")]
            public string Description { get; set; } = null!;

            [Required(ErrorMessage = "Kategori gereklidir")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Kategori 3-50 karakter arasında olmalıdır")]
            [Display(Name = "Kategori")]
            public string Category { get; set; } = null!;

            [Display(Name = "Aktif")]
            public bool IsActive { get; set; }
        }

        /// <summary>
        /// Assign Permission to Role DTO
        /// </summary>
        public class AssignPermissionDto
        {
            [Required(ErrorMessage = "Role ID gereklidir")]
            [Display(Name = "Role ID")]
            public int RoleId { get; set; }

            [Required(ErrorMessage = "Permission ID gereklidir")]
            [Display(Name = "Permission ID")]
            public int PermissionId { get; set; }
        }

        /// <summary>
        /// Remove Permission from Role DTO
        /// </summary>
        public class RemovePermissionDto
        {
            [Required(ErrorMessage = "Role ID gereklidir")]
            [Display(Name = "Role ID")]
            public int RoleId { get; set; }

            [Required(ErrorMessage = "Permission ID gereklidir")]
            [Display(Name = "Permission ID")]
            public int PermissionId { get; set; }
        }
    }
}