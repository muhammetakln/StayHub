using System.ComponentModel.DataAnnotations;
public partial class RoleDto
{
    
    public class UserRoleDto
    {
        [Display(Name = "ID")]
        public int Id { get; set; }

        [Display(Name = "Guest ID")]
        public int GuestId { get; set; }

        [Display(Name = "Guest Adı")]
        public string GuestName { get; set; } = null!;

        [Display(Name = "Role ID")]
        public int RoleId { get; set; }

        [Display(Name = "Role Adı")]
        public string RoleName { get; set; } = null!;

        [Display(Name = "Atama Tarihi")]
        public DateTime AssignedAt { get; set; }


        /// <summary>
        /// Assign Role to User DTO
        /// </summary>
        public class AssignRoleToUserDto
        {
            [Required(ErrorMessage = "Guest ID gereklidir")]
            [Display(Name = "Guest ID")]
            public int GuestId { get; set; }

            [Required(ErrorMessage = "Role ID gereklidir")]
            [Display(Name = "Role ID")]
            public int RoleId { get; set; }
        }

        /// <summary>
        /// Remove Role from User DTO
        /// </summary>
        public class RemoveRoleFromUserDto
        {
            [Required(ErrorMessage = "Guest ID gereklidir")]
            [Display(Name = "Guest ID")]
            public int GuestId { get; set; }

            [Required(ErrorMessage = "Role ID gereklidir")]
            [Display(Name = "Role ID")]
            public int RoleId { get; set; }
        }
    }
}