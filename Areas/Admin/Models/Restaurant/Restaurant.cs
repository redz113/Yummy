using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppFoods.Models
{
    public class Restaurant{
        [Key]
        public int Id {get; set;}

        [Display(Name = "Tên cơ sở")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(100)]
        public string Name {get; set;}

        [Display(Name = "Địa chỉ")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(255)]
        public string Address {get; set;}

        [Display(Name = "Thành phố")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(100)]
        public string City {get; set;}

        [Display(Name = "Số tầng")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public int FloorNumber {get; set;}          //S? t?ng

        [Display(Name = "Số bàn")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public int TableNumber {get; set;}          //S? b�n
         
        [Display(Name = "Nhân viên quản lý")]
        public string ManagerId {get; set;}
        [Display(Name = "Nhân viên quản lý")]
        [ForeignKey("ManagerId")]
        public AppUser? User {get; set;}

        // public ICollection<Table> ChildrenTables {get; set;}
    }
}