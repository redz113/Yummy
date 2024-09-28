using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppFoods.Models;
using Microsoft.AspNetCore.Identity;
using App.Data;
using NuGet.Protocol;
using Microsoft.AspNetCore.Authorization;
using AppFoods.Data;

namespace AppFoods.Areas_Admin_Controllers
{
    [Authorize(Policy = ClaimName.RestaurantManager)]
    [Area("Admin")]
    [Route("/chuoi-co-so/[Action]/{id?}")]
    public class RestaurantController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public RestaurantController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Restaurant
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Restaurant.Include(r => r.User);
            return View(await appDbContext.ToListAsync());
        }

        private async Task<SelectList> _GetListManager(){
            var manager = (await _userManager.GetUsersInRoleAsync(RoleName.Manager)).Select(u => new {u.Id, u.Name}).ToList();
            manager.Insert(0, new {
                Id = "-1",
                Name =  "Chọn nhân viên quản lý"
            });
            
            return  new SelectList(manager, "Id", "Name", "-1");
        }

        // GET: Restaurant/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant
                .Include(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);
        }

        // GET: Restaurant/Create
        public async Task<IActionResult> Create()
        {

            ViewData["ManagerId"] = await _GetListManager();
            return View();
        }

        // POST: Restaurant/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Address,City,FloorNumber,TableNumber,ManagerId")] Restaurant restaurant)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ManagerId"] = await _GetListManager();
                return View(restaurant);
            }

            if(restaurant.ManagerId == "-1"){
                ViewData["ManagerId"] = await _GetListManager();
                TempData["error"] = "Cần chọn nhân viên quản lý.";
                return View(restaurant);
            }

            var res = await _context.Restaurants.FirstOrDefaultAsync(r => r.Name == restaurant.Name || r.Address == restaurant.Address);
            if(res != null){
                ViewData["ManagerId"] = await _GetListManager();
                TempData["error"] = "Cơ sở đã có trên hệ thống.";
                return View(restaurant);
            }

            _context.Add(restaurant);
            await _context.SaveChangesAsync();

            var id = (await _context.Restaurants.OrderBy(r => r.Id).LastAsync()).Id;
            await _AddTable(restaurant.FloorNumber, restaurant.TableNumber, id);
            TempData["success"] = "Đã thêm 1 cơ sở mới";
            return RedirectToAction(nameof(Index));
           
        }

        private async Task _AddTable(int floor, int TableNumber, int restaurantId){
            var tableOdd = await _context.Tables.Where(t => t.RestaurantId == restaurantId).ToListAsync();
            if(tableOdd.Count() > 0){
                _context.Tables.RemoveRange(tableOdd);
                await _context.SaveChangesAsync();
            }

            int tableInFloor = (int) Math.Ceiling((double) TableNumber / (double) floor);
            List<Table> tables = new List<Table>();
            for(int i=1; i<= floor; i++){
                for(int j = 1; j <= tableInFloor; j++){
                    if(TableNumber > 0){
                        tables.Add(new Table(){
                            Name = j.ToString(),
                            Location = i,
                            RestaurantId = restaurantId,
                            Vip = false
                        });
                        TableNumber--;
                    }else{
                        break;
                    }
                }
            }
            await _context.Tables.AddRangeAsync(tables);     
            await _context.SaveChangesAsync();                          
        }

        // GET: Restaurant/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var restaurant = await _context.Restaurant.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            ViewData["ManagerId"] = await _GetListManager();
            return View(restaurant);
        }

        // POST: Restaurant/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Address,City,FloorNumber,TableNumber,ManagerId")] Restaurant restaurant)
        {
            if (id != restaurant.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["error"] = "Lỗi";
                ViewData["ManagerId"] = await _GetListManager();
                return View(restaurant);
            }

            if(restaurant.ManagerId == "-1"){
                ViewData["ManagerId"] = await _GetListManager();
                TempData["error"] = "Cần chọn nhân viên quản lý.";
                return View(restaurant);
            }

            var res = await _context.Restaurants.FirstOrDefaultAsync(r => (r.Name == restaurant.Name || r.Address == restaurant.Address) && r.Id != id);
            if(res != null){
                ViewData["ManagerId"] = await _GetListManager();
                TempData["error"] = "Cơ sở đã có trên hệ thống.";
                return View(restaurant);
            }

            var restaurantOld = await _context.Restaurants.Where(r => r.Id == id).Select(r => new {r.FloorNumber, r.TableNumber}).FirstOrDefaultAsync();
            try
            {
                _context.Update(restaurant);
                await _context.SaveChangesAsync();
                if(restaurantOld.FloorNumber != restaurant.FloorNumber || restaurantOld.TableNumber != restaurant.TableNumber){
                    await _AddTable(restaurant.FloorNumber, restaurant.TableNumber, id);
                }
                TempData["success"] = "Đã cập nhật thông tin cơ sở";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantExists(restaurant.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
            
        }

        public async Task<IActionResult> InfoManager(int id, string userId){
            ViewBag.Id = id;
            return View(await _context.Users.FirstOrDefaultAsync(u => u.Id == userId));
        }

        // POST: Restaurant/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var restaurant = await _context.Restaurant.FindAsync(id);
            if (restaurant != null)
            {
                _context.Restaurant.Remove(restaurant);

                var tables = await _context.Tables.Where(t => t.RestaurantId == id).ToListAsync();
                if(tables.Count() > 0){
                    _context.Tables.RemoveRange(tables);
                }
            }

            await _context.SaveChangesAsync();
            TempData["success"] = "Đã xóa thông tin cơ sở";
            return RedirectToAction(nameof(Index));
        }

        private bool RestaurantExists(int id)
        {
            return _context.Restaurant.Any(e => e.Id == id);
        }
    }
}
