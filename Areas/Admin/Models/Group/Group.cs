using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppFoods.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tên nhóm")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public string Name { get; set; }

        public int? Arrange {get; set;}

        public ICollection<Group>? ChildrenGroup { get; set; }

        [Display(Name = "Nhóm cha")]
        public int? ParentId { get; set; }

        [Display(Name = "Nhóm cha")]
        [ForeignKey(nameof(ParentId))]
        public Group? Parent { get; set; }
    }
}
