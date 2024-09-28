using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppFoods.Models
{
    public class Menu {
        [Key]
        public int Id {get; set;}

        [Column(TypeName = "NVARCHAR")]
        [StringLength(255)]
        [Display(Name = "Tên món ăn")]
        [Required(ErrorMessage = "Phải nhập thông tin {0}.")]
        public string Name {get; set;}

        [Column(TypeName = "NVARCHAR")]
        [StringLength(500)]
        [Display(Name = "Mô tả")]
        [Required(ErrorMessage = "Phải nhập thông tin {0}.")]
        public string Description {get; set;}

        [Display(Name = "Giá")]
        [DataType(DataType.Currency)]
        [Required(ErrorMessage = "Phải nhập thông tin {0}.")]
        public int Price {get; set;}

        [StringLength(100)]
        public string? ImageSrc {get; set;} = "/img/avt/noimage.png";


        [Display(Name = "Tên nhóm")]
        public int GroupId { get; set; }
        [ForeignKey(nameof(GroupId))]
        public Group? Group { get; set; }
    }
}