using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppFoods.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppFoods.Models
{
    [Bind("Id","UserId","Level","Description","Salary","ServicePrice")]
    public class Chef {
        [Key]
        public int Id {get; set;}

        [Display(Name = "Đầu bếp")]
        public string UserId{get; set;}
        [ForeignKey("UserId")]
        public AppUser? User{get; set;}


        [Column(TypeName = "NVARCHAR")]
        [Display(Name = "Chuyên môn chính")]
        [StringLength(50)]
        [Required(ErrorMessage = "Phải nhập thông tin {0}.")]
        public string Level {get; set;}

        [Column(TypeName = "NVARCHAR")]
        [Display(Name = "Mô tả")]
        [StringLength(500)]
        [Required(ErrorMessage = "Phải nhập thông tin {0}.")]
        public string Description {get; set;}

        [Display(Name = "Lương")]
        [DataType(DataType.Currency)]
        public int Salary {get; set;}

        [Display(Name = "Cơ sở làm việc")]
        public int RestaurantId {get; set;}
        public Restaurant? Restaurant {get; set;}

    }


    public class ChefModel
    {
        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public string Name {get; set;}

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [EmailAddress(ErrorMessage = "{0} không hợp lệ")]
        public string Email {get; set;}

        public string Password {get;} = "123456";

        [Display(Name = "Ngày sinh")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public DateTime? DateOfBirth {get; set;} = null;

        [Display(Name = "Giới tính")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        public int Gender {get; set;}

        [Display(Name = "Cơ sở làm việc")]
        public int RestaurantId {get; set;}
        public Chef Chef {get; set;}
    }

    public class ChefDetailsModel
    {
        public string UserId {get; set;}
        public Chef Chef {get; set;}
    }
}