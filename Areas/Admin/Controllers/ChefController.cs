using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppFoods.Models;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using AppFoods.Data;
using Microsoft.AspNetCore.Identity;
using NuGet.Protocol;

namespace AppFoods.Areas_Admin_Controllers
{
    [Authorize(Policy = ClaimName.ChefManager)]
    [Area("Admin")]
    [Route("/thong-tin-dau-bep/{action=Index}/{id?}")]
    public class ChefController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogger<ChefController> _logger;

        public List<string> Levels = new List<string>{
            "Chuyên về các món Hải sản", "Chuyên về các món Nướng", 
            "Chuyên về các món Salad, Rau củ", "Chuyên về Bánh ngọt, Tráng miệng", 
        };

        public ChefController(AppDbContext context, UserManager<AppUser> userManager, ILogger<ChefController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // GET: Chef
        public async Task<IActionResult> Index()
        {
            var chefs = await _GetChef();   
            return View(chefs);
        }

        private async Task<List<AppUser>> _GetChef(){
            var userId =    await (from u in _context.UserRoles
                            join r in _context.Roles on u.RoleId equals r.Id
                            where r.Name == RoleName.Chef
                            select u.UserId).ToListAsync();

            var chef =  await _context.Users
                            .Include(u => u.ChildrenChef)
                            .Where(u => userId.Contains(u.Id)).ToListAsync();

                            
            return chef;
        }

        private async Task<SelectList> _GetSelectRestaurant(int? restaurantId = null){
            var res =  await _context.Restaurants.ToListAsync();
            if(restaurantId != null){
                return new SelectList(res, "Id", "Name", restaurantId);
            }
            return new SelectList(res, "Id", "Name");
        }



        // GET: Chef/Details/5
        public async Task<IActionResult> Details(string? id)
        {
           var user = await _context.Users.Select(c => new {c.Id, c.Name, c.Email, c.DateOfBirth, c.Gender, c.AvatarImg}).FirstOrDefaultAsync(c => c.Id == id);

            if(user == null){
                return NotFound();
            }

            var chef = await _context.Chefs
                        .Include(c => c.Restaurant)
                        .FirstOrDefaultAsync(c => c.UserId == id);
            
            if(chef == null) chef = new Chef();

            ChefDetailsModel model = new ChefDetailsModel(){
                UserId = id,
                Chef = chef
            };
            
            ViewBag.UserInfo = user;
            return View(model);
        }

        // GET: Chef/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.SelectRestaurant = await _GetSelectRestaurant();
            ViewBag.Levels = new SelectList(Levels);    

            return View();
        }

        // POST: Chef/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(ChefModel model)
        {
            if(model.Gender == -1){
                ModelState.AddModelError("Gender", "Yêu cầu chọn");
            }

            if(model.Chef.Level == "-1"){
                ModelState.AddModelError("Chef.Level", "Yêu cầu chọn");
            }

            if(model.RestaurantId == -1){
                ModelState.AddModelError("RestaurantId", "Yêu cầu chọn");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.SelectRestaurant = await _GetSelectRestaurant();
                ViewBag.Levels = new SelectList(Levels);    
                return View(model);
            }

            AppUser user = new AppUser(){
                Name = model.Name,
                UserName = Guid.NewGuid().ToString(),
                Email = model.Email,
                Gender = (byte) model.Gender,
                DateOfBirth = model.DateOfBirth,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if(!result.Succeeded){
                TempData["error"] = "Email đã được đăng ký với một tài khoản có trên hệ thống.";
                return View(model);
            }else{
                await _userManager.AddToRoleAsync(user, RoleName.Chef);
                model.Chef.UserId = user.Id;
                model.Chef.RestaurantId = model.RestaurantId;
                await _context.Chefs.AddAsync(model.Chef);
                await _context.SaveChangesAsync();
            }
            
            TempData["success"] = "Đã thêm thông tin đầu bếp";
            return RedirectToAction("Index");
        }

        // GET: Id - UserId
        public async Task<IActionResult> Edit(string? id)
        {
            var user = await _context.Users.Select(c => new {c.Id, c.Name, c.Email, c.DateOfBirth, c.Gender, c.AvatarImg}).FirstOrDefaultAsync(c => c.Id == id);

            if(user == null){
                return NotFound();
            }

            var chef = await _context.Chefs
                        .FirstOrDefaultAsync(c => c.UserId == id);
            
            if(chef == null) chef = new Chef(){
                Description = "",
                Level = "",
                Salary = 0
            };

            ChefDetailsModel model = new ChefDetailsModel(){
                UserId = id,
                Chef = chef
            };
            
            ViewBag.UserInfo = user;
            ViewBag.SelectRestaurant = await _GetSelectRestaurant(chef.RestaurantId);
            ViewBag.Levels = new SelectList(Levels);  
            return View(model);
        }

        // POST: Chef/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ChefDetailsModel model)
        {
           
            model.Chef.RestaurantId = Int32.Parse(Request.Form["restaurantId"]);

            if(model.Chef.Level == "-1"){
                ModelState.AddModelError("Chef.Level", "Yêu cầu chọn");
            }

            if(model.Chef.RestaurantId == -1){
                ModelState.AddModelError("Chef.RestaurantId", "Yêu cầu chọn");
            }

            if(ModelState.IsValid){
                 try
                {
                    if(model.Chef.Id == 0){
                        await _context.AddAsync(model.Chef);
                    }else{
                        _context.Update(model.Chef);
                    }
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ChefExists(model.Chef.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                TempData["Success"] = "Đã cập nhật thông tin";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.UserInfo = await _context.Users.Select(c => new {c.Id, c.Name, c.Email, c.DateOfBirth, c.Gender, c.AvatarImg}).FirstOrDefaultAsync(c => c.Id == model.UserId);
            ViewBag.SelectRestaurant = await _GetSelectRestaurant(model.Chef.RestaurantId);
            ViewBag.Levels = new SelectList(Levels);  
            return View(model);
        }

        [Authorize(Roles = RoleName.Administrator)]
        // POST: Chef/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            return Content(id);
            // var chef = await _context.Chefs.FindAsync(id);
            // if (chef != null)
            // {
            //     _context.Chefs.Remove(chef);
            // }

            // await _context.SaveChangesAsync();
            // return RedirectToAction(nameof(Index));
        }

        private bool ChefExists(int id)
        {
            return _context.Chefs.Any(e => e.Id == id);
        }


        private async Task<List<AppUser>> _GetListChef(){
            var userId = from r in _context.Roles
                        join u in _context.UserRoles on r.Id equals u.RoleId
                        where r.Name == RoleName.Chef
                        select u.UserId;

            var User = await _context.Users
                        .Where(u => userId.Contains(u.Id))
                        .Select(u => u).ToListAsync();

            return User;
        }
    
    }
}
