using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppFoods.Models
{
    public class Table
    {
        [Key]
        public int Id { get; set; }
        [Display(Name = "Tên bàn")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(100)]
        public string Name { get; set; }

        [Display(Name = "Vị trí")]
        [Required(ErrorMessage = "Phải chọn {0}")]
        public int Location { get; set; }

        [Display(Name = "Loaị bàn")]
        [Required(ErrorMessage = "Phải chọn {0}")]
        public bool Vip { get; set; } = false;

        [Display(Name = "Trạng thái")]
        [Required(ErrorMessage = "Phải chọn {0}")]
        public bool Status {get; set;} = false;         // Trạng thái: True - Đang được sử dụng | False - Bàn trống

        [Display(Name = "Thời gian")]
        public DateTime? TimmOn {get; set;} = DateTime.Now;

        [Display(Name = "Tên cơ sở")]
        public int RestaurantId { get; set; } 

        [Display(Name = "Tên cơ sở")]
        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }

        public ICollection<MenuOrder>? ChildrerMenuOrders {get; set;}
        public ICollection<ComboOrder>? ChildrerComboOrders {get; set;}
    }
}
