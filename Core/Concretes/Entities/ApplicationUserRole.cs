using Microsoft.AspNetCore.Identity;

namespace Core.Concretes.Entities
{
    public class Role : IdentityRole
    {
        public string?  Description { get; set; }
    }
}