using Bogus.DataSets;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppFoods.Models
{
    public class AppUser : IdentityUser
    {
        [PersonalData]
        [StringLength(100)]
        [Column(TypeName = "NVARCHAR")]
        public string Name { get; set; }

        [PersonalData]
        public DateTime? DateOfBirth { get; set; }

        [PersonalData]
        public byte? Gender { get; set; }               // 0 - female | 1 - Male | 2 - Other

        [PersonalData]
        [StringLength(255)]
        [Column(TypeName = "NVARCHAR")]
        public string? Address { get; set; }

        [PersonalData]
        public string? AvatarImg { get; set; }

        public ICollection<Chef>? ChildrenChef {get; set;}
        public ICollection<Restaurant>? ChildrenRestaurant {get; set;}

        public override string ToString()
        {
            return $"{Name} - {Email}";
        }
    }
}
