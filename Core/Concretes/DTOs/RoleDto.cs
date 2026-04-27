using System.ComponentModel.DataAnnotations;
/// <summary>
/// Role DTO (List)
/// </summary>
public partial class RoleDto
{
    [Display(Name = "ID")]
    public int Id { get; set; }

    [Display(Name = "Rol Adı")]
    public string Name { get; set; } = null!;

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Display(Name = "Aktif")]
    public bool IsActive { get; set; }

    [Display(Name = "Kullanıcı Sayısı")]
    public int UserCount { get; set; }

    [Display(Name = "Oluşturulma Tarihi")]
    public DateTime CreatedAt { get; set; }


    /// <summary>
    /// Role Detail DTO
    /// </summary>
    public class RoleDetailDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Rol Adı")]
        public string Name { get; set; } = null!;

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Permissions")]
        public ICollection<PermissionDto>? Permissions { get; set; } = new List<PermissionDto>();

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Güncellenme Tarihi")]
        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Create Role DTO
    /// </summary>
    public class CreateRoleDto
    {
        [Required(ErrorMessage = "Rol adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Rol adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Rol Adı")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Açıklama 500 karakteri geçemez")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Permission ID'leri")]
        public List<int>? PermissionIds { get; set; } = new List<int>();
    }

    /// <summary>
    /// Update Role DTO
    /// </summary>
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "Rol adı gereklidir")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Rol adı 3-100 karakter arasında olmalıdır")]
        [Display(Name = "Rol Adı")]
        public string Name { get; set; } = null!;

        [StringLength(500, ErrorMessage = "Açıklama 500 karakteri geçemez")]
        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Aktif")]
        public bool IsActive { get; set; }

        [Display(Name = "Permission ID'leri")]
        public List<int>? PermissionIds { get; set; } = new List<int>();
    }
}