using App.Data;
using AppFoods.Data;
using AppFoods.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace AppFoods.Controllers
{
    [Authorize(Policy = ClaimName.OrderManager)]
    [Area("Admin")]
    [Route("/dán-sach-don/[Action]/{id?}")]
    public class OrderController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public OrderController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> IndexAsync(int restaurantId = -1){

            List<int> restaurantIds = await _GetRestaurantIdOfUser();

            if(restaurantIds.Count == 0){
                return View(new Order());
            }

            if(restaurantId == -1){
                restaurantId = restaurantIds.First();
            }

            if(!restaurantIds.Contains(restaurantId)){
                return NotFound();
            }
            
            List<int> tableIds =  await _GetTableIdInRestaurant(restaurantId);


            List<ComboOrder> comboOrders = await _context.ComboOrders  
                                             .Include(c => c.Combo)
                                             .Include(c => c.Table)
                                             .Where(c => tableIds.Contains(c.TableId) && c.Status == false)
                                             .OrderBy(c => c.Time)
                                             .ToListAsync();

            List<MenuOrder> menuOrders = await _context.MenuOrders  
                                             .Include(c => c.Menu)
                                             .Include(c => c.Table)
                                             .Where(c => tableIds.Contains(c.TableId) && c.Status == false)
                                             .OrderBy(c => c.Time)
                                             .ToListAsync();

            if(restaurantIds.Count > 1){
                ViewBag.CurrentRestaurant = restaurantId;
                ViewBag.Restaurants = await _context.Restaurants.Where(r => restaurantIds.Contains(r.Id)).ToListAsync();
            }

            Order order = new Order(){
                ComboOrders = comboOrders,
                MenuOrders = menuOrders
            };
            
            return View(order);
        }


        // LẤY ID CÁC CHI NHÁNH MÀ USER HIỆN TẠI ĐANG QUẢN LÝ | LÀM VIỆC
        private async Task<List<int>> _GetRestaurantIdOfUser(){
            var user = await _userManager.GetUserAsync(User);
            var roleNames = await _userManager.GetRolesAsync(user);

            List<int> restaurantId = new List<int>();
            if(roleNames.Contains(RoleName.Administrator)){
                var resId =  await _context.Restaurants
                                    .OrderBy(r => r.Id)
                                    .Select(r => r.Id)
                                    .ToListAsync();

                if(resId.Count > 0) restaurantId.AddRange(resId);

            }
            else if(roleNames.Contains(RoleName.Manager))
            {
                var resId = await _context.Restaurant
                                    .Where(r => r.ManagerId == user.Id)
                                    .Select(r => r.Id)
                                    .ToListAsync();
                if(resId.Count > 0) restaurantId.AddRange(resId);
            }
            else if(roleNames.Contains(RoleName.Chef))
            {
                var resId = await _context.Chefs
                                .Where(c => c.UserId == user.Id)
                                .Select(c => c.RestaurantId)
                                .FirstOrDefaultAsync();

                if(resId != null) restaurantId.Add(resId);

            }

            return restaurantId;
        }

        // private async Task<List<Table>> _GetTableInRestaurant(int restaurantId){

        //     return  await _context.Tables
        //             .Include(t => t.ChildrerComboOrders)
        //             .Include(t => t.ChildrerMenuOrders)
        //             .Where(t => restaurantId == t.RestaurantId)
        //             .ToListAsync();

        // }

        // LẤY ID CÁC BÀN MÀ CHI NHÁNH ĐANG SỞ HỮU
        private async Task<List<int>> _GetTableIdInRestaurant(int restaurantId){

            return  await _context.Tables
                    .Where(t => restaurantId == t.RestaurantId && t.Status == true)
                    .Select(t => t.Id)
                    .ToListAsync();

        }


    }
}