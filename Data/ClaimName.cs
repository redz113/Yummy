using System.ComponentModel.DataAnnotations;

namespace AppFoods.Data
{
    public class ClaimName
    {
        [Display(Name = "Quản lý tài khoản")]
        public const string UserManager = "Quản lý tài khoản";

        [Display(Name = "Quản lý món ăn")]
        public const string MenuManager = "Quản lý món ăn";

        [Display(Name = "Quản lý vai trò")]
        public const string RoleManager = "Quản lý vai trò";

        [Display(Name = "Quản lý đầu bếp")]
        public const string ChefManager = "Quản lý đầu bếp";

        [Display(Name = "Quản lý nhóm")]
        public const string GroupManager = "Quản lý nhóm";
        [Display(Name = "Quản lý bàn đặt")]
        public const string TableManager = "Quản lý bàn đặt";

        [Display(Name = "Quản lý đơn đặt")]
        public const string OrderManager = "Quản lý đơn đặt";

        [Display(Name = "Quản lý cơ sở")]
        public const string RestaurantManager = "Quản lý cơ sở";


        public static List<string>  ListClaims()
        {
            var claims = typeof(ClaimName).GetFields().ToList();
            List<string> rt = new List<string>();
            foreach (var claim in claims)
            {
                rt.Add(claim.Name);
            }
            return rt;
        }

        public static Dictionary<string, string> DirClaims()
        {
            Dictionary<string, string> dir = new Dictionary<string, string>();
            var claims = typeof(ClaimName).GetFields().ToList();
            foreach (var claim in claims)
            {
                dir.Add((string)claim.GetValue(0), claim.Name);
            }
            return dir;
        }
    }
}
