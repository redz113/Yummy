using AppFoods.Validations;
using System.ComponentModel.DataAnnotations;

namespace AppFoods.Areas.Identity.Models.User
{
    public class AddUserModel
    {
        
        [Display(Name = "Họ tên")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 3)]
        public string Name { get; set; }
        

        [DataType(DataType.EmailAddress)]
        [Display(Name = "Email")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [EmailAddress(ErrorMessage = "{0} không hợp lệ.")]
        public string? Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(100, ErrorMessage = "{0} phải dài từ {2} đến {1} ký tự.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; } = string.Empty;

        //[Display(Name = "Vai trò")]
        //public string[] Roles { get; set; }

        public List<bool>? RoleChecks { get; set; }
    }
}
