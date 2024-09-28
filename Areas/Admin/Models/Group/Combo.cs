using System.ComponentModel.DataAnnotations;

namespace AppFoods.Models
{
    public class Combo
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Tên combo")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(255)]
        public string Name { get; set; }

        [Display(Name = "Giá combo")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [DataType(DataType.Currency)]
        public int Price { get; set; }

        [Display(Name = "Trạng thái hiển thị")]     // True - Bật | False - Tắt
        public bool Status {get; set;}

        [StringLength(100)]
        public string? ImageSrc { get; set; }

        public string? Menu { get; set; }
    }
}
