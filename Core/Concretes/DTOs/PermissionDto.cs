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
       
    }
}