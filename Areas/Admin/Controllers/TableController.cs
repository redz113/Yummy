using App.Data;
using AppFoods.Data;
using AppFoods.Models;
using AppFoods.Services;
using AppFoods.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using NuGet.Protocol;
using Org.BouncyCastle.Ocsp;

namespace AppFoods.Controllers
{

    [Authorize(Policy = ClaimName.TableManager)]
    [Area("Admin")]
    [Route("/danh-sach-ban/[Action]/{id?}")]
    public class TableController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPDFService _pdfService;

        public TableController(AppDbContext context, UserManager<AppUser> userManager, IPDFService pdfService)
        {
            _context = context;
            _userManager = userManager;
            this._pdfService = pdfService;
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

        public async Task<IActionResult> IndexAsync(int location = 1, int restaurantId = -1)
        {

            var restaurantIds = await _GetRestaurantIdOfUser();

            var restaurants = await _context.Restaurants.Where(r => restaurantIds.Contains(r.Id)).ToListAsync();

            Restaurant restaurant = new Restaurant();

            if(restaurants.Count > 0){
                if(restaurantId == -1){
                    restaurant = restaurants.First();
                }else {
                    restaurant = restaurants.FirstOrDefault(r => r.Id == restaurantId);
                }

                if(restaurant == null){

                    return NotFound();
                }

                List<Table> tables = new List<Table>();
                tables = await _context.Tables.Where(t => t.RestaurantId == restaurant.Id && t.Location == location).ToListAsync();
                ViewBag.Tables = tables;

                List<dynamic> locations = new List<dynamic>();
                for(int i=1; i <= restaurant.FloorNumber; i++){
                    locations.Add(new {
                        Id = i,
                        Name = "Tầng " + i                 
                        });
                }

                ViewBag.Locations = locations;
            }
            ViewBag.TotalOn = await _context.Tables.Where(t => t.RestaurantId == restaurantId && t.Status == true).CountAsync();
            ViewBag.Restaurants = restaurants.Select(r => new {r.Id, r.Name}).ToList();
            ViewBag.CurrentFloor = location;
           return View(restaurant);
        }
         
        public async Task<IActionResult> Details(int id = -1){
            if(id == -1){
                return NotFound();
            }

            Table table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == id);
            if(table == null) return NotFound();

            List<Pay> pays = await _context.ComboOrders
                        .Where(c => c.TableId == id && c.Time >= table.TimmOn)
                        .Select(c => new Pay {
                            Id = c.Id, 
                            Name = c.Combo.Name,
                            Price = c.Combo.Price,
                            Quantity = c.Quantity,
                            PxQ = c.Combo.Price * c.Quantity,
                            Status = c.Status,
                            Time = c.Time,
                            IsMenu = false
                            }).ToListAsync();

            List<Pay> menuPays = await _context.MenuOrders
                        .Where(c => c.TableId == id && c.Time >= table.TimmOn)
                        .Select(c => new Pay {
                            Id = c.Id, 
                            Name = c.Menu.Name,
                            Price = c.Menu.Price,
                            Quantity = c.Quantity,
                            PxQ = c.Menu.Price * c.Quantity,
                            Status = c.Status,
                            Time = c.Time,
                            IsMenu = true
                            }).ToListAsync();

            pays.AddRange(menuPays);

            ViewBag.Pays = pays;

            return View(table);
        }


        

        public async Task<IActionResult> Order(int id){
            var table = await _context.Tables.Where(t => t.Id == id).FirstOrDefaultAsync();
            if(table == null){
                return NotFound();
            }

            var combos = await _context.Combos.Where(c => c.Status == true).ToListAsync();
            var groups = await _GetListGroups();
            var menus = await _context.Menus.ToListAsync();
            // return RedirectToAction("Index", new {location = table.Location});
            ViewBag.Table = table;
            ViewBag.Combos = combos;
            ViewBag.Groups = groups;
            ViewBag.Menus = menus;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> OrderAsync(int id){
            int count = Request.Form.Count();
            if(count > 1){
                
                List<int> menuId = Request.Form.Keys.Where(k => k.StartsWith("menu-quantity-")).Select(k => Int32.Parse(k.Substring(14))).ToList();
                List<int> comboId = Request.Form.Keys.Where(k => k.StartsWith("combo-quantity-")).Select(k => Int32.Parse(k.Substring(15))).ToList();

                // Thay đổi trạng thái bàn
                if(menuId.Count() > 0 || comboId.Count() > 0){
                    Table table = await _context.Tables.FirstOrDefaultAsync(t => t.Id == id);
                    if(table == null){
                        return NotFound();
                    }

                    if(!table.Status){
                        table.Status = true;
                        table.TimmOn = DateTime.Now;
                    }

                    _context.Tables.Update(table);
                }


                List<ComboOrder> comboOrders = new List<ComboOrder>();
                List<MenuOrder> menuOrders = new List<MenuOrder>();
                if(comboId.Count() > 0){
                    foreach(int i in comboId){
                        var quantity = Request.Form["combo-quantity-"+i];
                        comboOrders.Add(new ComboOrder(){
                            TableId = id,
                            ComboId = i,
                            Quantity = Int32.Parse(quantity)
                        });
                    }

                    await _context.ComboOrders.AddRangeAsync(comboOrders);
                }

                if(menuId.Count() > 0){
                    foreach(int i in menuId){
                        var quantity = Request.Form["menu-quantity-"+i];
                        menuOrders.Add(new MenuOrder(){
                            TableId = id,
                            MenuId = i,
                            Quantity = Int32.Parse(quantity)
                        });
                    }

                    await _context.MenuOrders.AddRangeAsync(menuOrders);
                }
                
                await _context.SaveChangesAsync();
                TempData["success"] = "Đặt món thành công";
                return RedirectToAction(nameof(Details), new {id = id});
                
            }
            return RedirectToAction(nameof(Order), new {id = id});
        }

        public async Task<IActionResult> GetOrders(int tableId, int menuId, bool isMenu = true){
            Order order = new Order();
            if(isMenu == true){
                order.MenuOrders.Add(new MenuOrder{
                    Menu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == menuId),
                    MenuId = menuId,
                    TableId = tableId,
                    Quantity = 1,
                });
            }
            else
            {
                order.ComboOrders.Add(new ComboOrder{
                    Combo = await _context.Combos.FirstOrDefaultAsync(m => m.Id == menuId),
                    ComboId = menuId,
                    TableId = tableId,
                    Quantity = 1,
                });
            }
            return PartialView("_GetOrderPartial", order);
        }

        public async Task<IActionResult> DeleteOrder(int id, int tableId, bool isMenu){
            if(id == null || tableId == null || isMenu == null){
                return NotFound();
            }
            if(isMenu){
                var menuOrder = await _context.MenuOrders.FirstOrDefaultAsync(m => m.Id == id);
                if(menuOrder != null) _context.MenuOrders.Remove(menuOrder);
            }else{
                var comboOrder = await _context.ComboOrders.FirstOrDefaultAsync(m => m.Id == id);
                if(comboOrder != null) _context.ComboOrders.Remove(comboOrder);
            }
            await _context.SaveChangesAsync();
            TempData["success"] = "Xóa thành công";
            return RedirectToAction(nameof(Details), new {id = tableId});
        }


        async Task<List<Group>> _GetListGroups()
        {
            var query = _context.Groups
                        .Include(m => m.Parent)
                        .Include(m => m.ChildrenGroup);
            var groups = (await query.ToListAsync())
                            .Where(m => m.Parent == null)
                            .OrderBy(m => m.Arrange)
                            .ToList();

            List<Group> result = new List<Group>();
            foreach (var item in groups)
            {
                int level = 0;
                result = await _GetChildren(result, item, level);
            }

            return result;
        }

        async Task<List<Group>> _GetChildren(List<Group>? result, Group item, int level, int? currentId = null)
        {
            if (currentId != item.Id)
            {
                string prefix = string.Concat(Enumerable.Repeat("--", level));
                item.Name = prefix + " " + item.Name;
                result.Add(item);

                if ((item.ChildrenGroup != null) && (item.ChildrenGroup.Count() > 0))
                {
                    foreach (var child in item.ChildrenGroup.OrderBy(m => m.Arrange))
                    {
                        await _GetChildren(result, child, level + 1, currentId);
                    }
                }
            }
            return result;
        }

        public async Task<IActionResult> Cancel(int id = 0){
            if(id == 0){
                return NotFound();
            }
            Build build = await _GetBuild(id);
            if(build == null) return NotFound();

            int total = build.ComboOrders.Where(c => c.Status == true).ToList().Count();
            total += build.MenuOrders.Where(c => c.Status == true).ToList().Count();

            if(total > 0){
                TempData["error"] = "Không thể hủy bàn.";
                return RedirectToAction("Details", new {id = id});
            }

            _context.ComboOrders.RemoveRange(build.ComboOrders);
            _context.MenuOrders.RemoveRange(build.MenuOrders);
            
            build.Table.Status = false;
            build.Table.TimmOn = null;
            
            _context.Tables.Update(build.Table);

            await _context.SaveChangesAsync();

            TempData["success"] = "Đã hủy bàn.";
            return RedirectToAction("Index", new {location = build.Table.Location});
        }

        public async Task<IActionResult> PayConfirm(int id){
            Build? build = await _GetBuild(id);
            if(build == null) return NotFound();

            build = await _RemoveOrderNoConfirm(build);

            // LƯU BUILD
            int totalPriceMenu = build.MenuOrders.Sum(m => m.Quantity * m.Menu.Price);
            int totalPriceCombos = build.ComboOrders.Sum(m => m.Quantity * m.Combo.Price);
            Summary summary = new Summary(){
                RestaurantId = build.Table.RestaurantId,
                TableName = build.Table.Location + "." + build.Table.Name,
                TotalPrice = totalPriceCombos + totalPriceMenu
            };
            
            if(summary.TotalPrice > 0){
                await _context.Summarys.AddAsync(summary);
            }
            
            build.Table.Status = false;
            build.Table.TimmOn = null;
            _context.Tables.Update(build.Table);

            await _context.SaveChangesAsync();

            return RedirectToAction("Index", new {location = build.Table.Location});
        }

        private async Task<Build> _RemoveOrderNoConfirm(Build build){
            var comboOrderRM = build.ComboOrders.Where(c => c.Status == false).ToList();
            var menuOrderRM = build.MenuOrders.Where(c => c.Status == false).ToList();

            if(comboOrderRM.Count > 0){
                foreach(ComboOrder c in comboOrderRM){
                    build.ComboOrders.Remove(c);
                }
            }

            if(menuOrderRM.Count > 0){
                foreach(MenuOrder m in menuOrderRM){
                    build.MenuOrders.Remove(m);
                }
            }

            _context.ComboOrders.RemoveRange(comboOrderRM);
            _context.MenuOrders.RemoveRange(menuOrderRM);
            await _context.SaveChangesAsync();
            return build;
        }

        // Id - Table Id
        // LƯU DỮ LIỆU => XUẤT BUILD
        public async Task<IActionResult> ExportPdfBuild(int id)
        {
            Build? build = await _GetBuild(id);
            if(build == null) return NotFound();
            
            var comboOrderRM = build.ComboOrders.Where(c => c.Status == false).ToList();
            var menuOrderRM = build.MenuOrders.Where(c => c.Status == false).ToList();

            if(comboOrderRM.Count > 0){
                foreach(ComboOrder c in comboOrderRM){
                    build.ComboOrders.Remove(c);
                }
            }

            if(menuOrderRM.Count > 0){
                foreach(MenuOrder m in menuOrderRM){
                    build.MenuOrders.Remove(m);
                }
            }

            if(build.ComboOrders.Count == 0 && build.MenuOrders.Count == 0){
                return RedirectToAction("Details", new {id = build.Table.Id});
            }

            string html =  await this.RenderViewAsync<object>(RouteData ,"_BuildPartial", build, true);
            var result = _pdfService.GeneratePDF(html, DinkToPdf.Orientation.Portrait, DinkToPdf.PaperKind.A6);
            return File(result, "application/pdf", $"build-{DateTime.Now.Ticks}.pdf");
        }

        public async Task<IActionResult> Build(int id){
            var build = await _GetBuild(id);
            if(build == null) return NotFound();

            return View(build); 
        }

        private async Task<Build?> _GetBuild(int id){
            var table = await _context.Tables
                        .Include(t => t.Restaurant)
                        .FirstOrDefaultAsync(t => t.Id == id);

            if(table == null) return null;
            var comboOrders = await _context.ComboOrders
                                .Include(c => c.Combo)
                                .Where(c => c.TableId == table.Id && c.Time > table.TimmOn)
                                .ToListAsync();
            var menuOrders = await _context.MenuOrders
                                .Include(c => c.Menu)
                                .Where(c => c.TableId == table.Id && c.Time > table.TimmOn)
                                .ToListAsync();

            return new Build(){
                Table = table,
                MenuOrders = menuOrders,
                ComboOrders = comboOrders
            };
        }

    }
}