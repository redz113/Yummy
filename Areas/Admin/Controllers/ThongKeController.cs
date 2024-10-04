using App.Data;
using AppFoods.Models;
using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol;
using Org.BouncyCastle.Asn1.Cms;

namespace AppFoods.Controllers
{
    [Authorize(Roles = $"{RoleName.Administrator},{RoleName.Manager}")]
    [Area("Admin")]
    [Route("/thong-ke/[Action]/{id?}")]
    public class ThongKeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ThongKeController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> IndexAsync(int restaurantId = -1){
            List<int> restaurants = await _GetRestaurantIdOfUser();
            if(restaurants.Count == 0){
                TempData["success"] = "Ban chưa được cấp quyền";
                return RedirectToAction("Index", "Home");
            }
            
            if(restaurantId == -1){
                restaurantId = restaurants.First();
            }

            DateTime now = DateTime.Now;
            List<ThongKe> model = new List<ThongKe>();
            foreach(int id in restaurants){
                var isRes = await _context.Restaurants.Where(r => r.Id == id).FirstOrDefaultAsync();
                var query = await _context.Summarys
                                .Where(s => s.RestaurantId == id && s.Time.Year == now.Year)
                                .ToListAsync();
                if(query.Count > 0){
                    int totalYear = query.Sum(s => s.TotalPrice);
                    int totalMonth = query.Where(s => s.Time.Month == now.Month).Sum(s => s.TotalPrice);
                    int totalDay = query.Where(s => s.Time.Month == now.Month && s.Time.Day == now.Day).Sum(s => s.TotalPrice);

                    model.Add(new ThongKe(){
                        Restaurant = isRes,
                        TotalYear = totalYear,
                        TotalMonth = totalMonth,
                        TotalDay = totalDay,
                        scaleDay = 0,
                        scaleMonth = 0,
                        scaleYear = 0
                    });
                }
                else{
                    model.Add(new ThongKe(){
                        Restaurant = isRes,
                        TotalYear = 0,
                        TotalMonth = 0,
                        TotalDay = 0,
                        scaleDay = 0,
                        scaleMonth = 0,
                        scaleYear = 0
                    });
                }
                
                // int preTotalMonth = query.Where(s => s.Time.Month == now.Month - 1).Sum(s => s.TotalPrice);
            }

            ViewBag.Restaurants = await _context.Restaurants.Where(r => restaurants.Contains(r.Id)).ToListAsync();
            ViewBag.CurrentRestaurant = restaurantId;
            return View(model);
        }


        // AJAX
        [Route("/thong-ke/AjaxGetDataMonthSummary")]
        public async Task<JsonResult> AjaxGetDataMonthSummary(int restaurantId, int year){
            var restaurantIds = await _GetRestaurantIdOfUser();
            List<DataThongKeNgay> result = new List<DataThongKeNgay>();

            foreach(int id in restaurantIds){
                string resName = await _context.Restaurants.Where(r => r.Id == id).Select(r => r.Name).FirstAsync();
                List<DataThongKe> data = await MonthInYearSummary(id, year);
                result.Add(new DataThongKeNgay(){
                    Name = resName,
                    Data = data
                });
            }

            // var result = await MonthInYearSummary(restaurantId, year);

            return Json(result);
        }

        // AJAX
        [Route("/thong-ke/AjaxGetDataDaySummary")]
        public async Task<JsonResult>  AjaxGetDataDaySummary(int restaurantId, int month){
            var result = await DayInMonthSummary(restaurantId, month);
              
            return Json(result);
        }

        // AJAX
        [Route("/thong-ke/AjaxGetDoanhThu")]
        public async Task<JsonResult> AjaxGetDoanhThu(int restaurantId){
            List<DataThongKe> data = await YearSummary(restaurantId);
            return Json(data);
        }

        // Thống kê theo ngày trong tháng
        private async Task<List<DataThongKe>> DayInMonthSummary(int restaurantId, int month){
            List<DataThongKe> result = await _context.Summarys
                            .Where(s => s.RestaurantId == restaurantId && s.Time.Year == DateTime.Now.Year && s.Time.Month == month)
                            .GroupBy(s => s.Time.Day)
                            .Select(s => new DataThongKe {Key = s.Key, Total = s.Sum(o => o.TotalPrice)})
                            .ToListAsync();

            int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, month);
            if(result.Count < daysInMonth){
                for(int i = 0; i < daysInMonth; i++){
                    var setVal = result.ElementAtOrDefault(i);
                    int number = i + 1;
                    if(setVal != null){
                        if(setVal.Key != number){
                            result.Insert(i, new DataThongKe(){
                                Key = number,
                                Total = 0
                            });
                        }
                    }
                    else{
                        result.Insert(i, new DataThongKe(){
                            Key = number,
                            Total = 0
                        });
                    }
                }
            }

            return result.OrderBy(m => m.Key).ToList();
        }

        // Thống kê theo tháng trong năm
        private async Task<List<DataThongKe>> MonthInYearSummary(int restaurantId, int year){
            List<DataThongKe> result = await _context.Summarys
                            .Where(s => s.RestaurantId == restaurantId && s.Time.Year == year)
                            .GroupBy(s => s.Time.Month)
                            .Select(s => new DataThongKe {Key = s.Key, Total = s.Sum(o => o.TotalPrice)})
                            .OrderBy(o => o.Key)
                            .ToListAsync();

            if(result.Count < 12){
                for(int i = 0; i < 12; i++){
                    var setVal = result.ElementAtOrDefault(i);
                    int number = i + 1;
                    if(setVal != null){
                        if(setVal.Key != number){
                            result.Insert(i, new DataThongKe(){
                                Key = number,
                                Total = 0
                            });
                        }
                    }
                    else{
                        result.Insert(i, new DataThongKe(){
                            Key = number,
                            Total = 0
                        });
                    }
                }
            }

            return result;
            
        }

        // Thống kê theo năm
        private async Task<List<DataThongKe>> YearSummary(int restaurantId){
            return await _context.Summarys
                            .Where(s => s.RestaurantId == restaurantId)
                            .GroupBy(s => s.Time.Year)
                            .Select(s => new DataThongKe {Key = s.Key, Total = s.Sum(o => o.TotalPrice)})
                            .OrderBy(s => s.Key)
                            .Take(4)
                            .ToListAsync();
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
    }
}