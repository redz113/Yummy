using App.Data;
using AppFoods.Data;
using AppFoods.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using System.Net.NetworkInformation;

namespace AppFoods.Controllers
{
    [Authorize(Policy = ClaimName.MenuManager)]
    [Route("/thuc-don/{action}")]
    [Area("Admin")]
    public class MenuController : Controller
    {
        // GET: MenuController
        private readonly ILogger<MenuController> _logger;
        private readonly AppDbContext _context;

        public MenuController(ILogger<MenuController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        // GET: MenuClassifyController
        [Route("/thuc-don")]
        public async Task<ActionResult> IndexAsync(int p = 1, IndexModel model = null)
        {
            if (model.SearchKey.IsNullOrEmpty())
            {
                model.SearchKey = "";
            }

            int totalRecord = _context.Menus.Where(m => m.Name.Contains(model.SearchKey.Trim())).Count();

            model.Paging = new Paging()
            {
                Action = "Index",
                TotalRecord = totalRecord,
                CurrentPage = p,
            };

            if (!model.SearchKey.IsNullOrEmpty())
            {
                model.Paging.Param += "&SearchKey=" + model.SearchKey;
            }

            var query = _context.Menus
                            .Include(m => m.Group)
                            .Where(m => m.Name.Contains(model.SearchKey.Trim()));

            if (model.GroupId != null && model.GroupId != -1)
            {
                var listGroupId = await _GetListGroupId(model.GroupId);

                totalRecord = _context.Menus.Where(m => m.Name.Contains(model.SearchKey.Trim()) && listGroupId.Contains(m.GroupId)).Count();
                query = _context.Menus
                    .Include(m => m.Group)
                    .Where(m => m.Name.Contains(model.SearchKey.Trim()) && listGroupId.Contains(m.GroupId));
                
                model.Paging.Param += "&GroupId=" + model.GroupId;
                model.Paging.TotalRecord = totalRecord;
            }

            //Default
            var ListMenu = (await query.ToListAsync())
                            .OrderBy(m => m.Group.Arrange);              // Sắp xếp theo tiêu chí đầu tiên
                            //.ThenBy(m => m.Group.Arrange);                   // ThenBy - trường hợp Classify trùng nhu
                            // .Skip((p - 1) * model.Paging.Limit)
                            // .Take(model.Paging.Limit);

            if (model.SortByName != null)
            {
                model.Paging.Param += "&SortByName=" + model.SortByName;
                if(model.SortByName == true)
                {
                   ListMenu = (await query.ToListAsync())
                                .OrderBy(m => m.Name);                   // ThenBy - trường hợp Classify trùng nhu
                                // .Skip((p - 1) * model.Paging.Limit)
                                // .Take(model.Paging.Limit);
                }
                else
                {
                    ListMenu = (await query.ToListAsync())
                                .OrderByDescending(m => m.Name);                   // ThenBy - trường hợp Classify trùng nhu
                                // .Skip((p - 1) * model.Paging.Limit)
                                // .Take(model.Paging.Limit);
                }
            }

            if (model.SortByPrice != null)
            {
                model.Paging.Param += "&SortByPrice=" + model.SortByPrice;
                if (model.SortByPrice == true)
                {
                    ListMenu = (await query.ToListAsync())
                                .OrderBy(m => m.Price);               // Sắp xếp theo tiêu chí đầu tiên
                                // .Skip((p - 1) * model.Paging.Limit)
                                // .Take(model.Paging.Limit);
                }
                else
                {
                    ListMenu = (await query.ToListAsync())
                                .OrderByDescending(m => m.Price);               // Sắp xếp theo tiêu chí đầu tiên
                                // .Skip((p - 1) * model.Paging.Limit)
                                // .Take(model.Paging.Limit);
                }
            }

           
            ViewBag.ListMenu = ListMenu.ToList()
                                .Skip((p - 1) * model.Paging.Limit)
                                .Take(model.Paging.Limit)
                                .ToList();

            ViewBag.Groups = await _GetSelectList();
            return View(model);
        }

        public async Task<IActionResult> CreateAsync(){
            ViewBag.Groups = await _GetSelectList();
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> CreateAsync(Menu model){
            
            if(!ModelState.IsValid){
                ViewBag.Groups = await _GetSelectList();
                TempData["error"] = "Thêm mới thất bại, Nhập đầy đủ thông tin.";
                return View(model);
            }

            if (model.GroupId == -1){
                ViewBag.Groups = await _GetSelectList();
                TempData["error"] = "Thêm mới thất bại, Chọn nhóm cho món ăn.";
                return View(model);
            }

            model.Name = model.Name.Trim().ToUpper();

            var menu = await _context.Menus.Where(m => m.Name == model.Name).FirstOrDefaultAsync();

            if(menu != null){
                TempData["Error"] = "Thêm mới thất bại, Tên món ăn đã tồn tại.";
                return View();
            }

            var file = Request.Form.Files.Count() > 0 ? Request.Form.Files["ImageSrc"] : null;
            if(file != null){
                string error = CheckFileUpload(file);
                if(!string.IsNullOrEmpty(error)){
                    TempData["Error"] = error;
                    return View();
                }

                string extension = Path.GetExtension(file.FileName).ToLower();
                string fileName = model.Name.Replace(" ", "_") + extension;
                string filePath = Path.Combine("wwwroot", "img", "menu", fileName);
                using(var stream = new FileStream(filePath, FileMode.Create)){
                    await file.CopyToAsync(stream);
                    model.ImageSrc = filePath.Substring(7);
                }
            }
            
            _context.Menus.Add(model);
            await _context.SaveChangesAsync();
            ViewBag.Groups = await _GetSelectList();
            TempData["Success"] = "Thêm món mới thành công.";
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> EditAsync(int id = 0){
            ViewBag.Groups = await _GetSelectList();
            var menu = await _context.Menus.FindAsync(id);
            if(menu == null){
                TempData["Error"] = "Cập nhật thất bại, món ăn không có trên hệ thống.";
                return View();
            }
            Menu model = menu;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditAsync(int id, Menu model){
            string? srcFileOld = await _context.Menus.Where(m => m.Id == model.Id).Select(m => m.ImageSrc).FirstOrDefaultAsync();
            if(!ModelState.IsValid){
                ViewBag.Groups = await _GetSelectList();
                TempData["error"] = "Cập nhật thất bại, Nhập đầy đủ thông tin.";
                model.ImageSrc = srcFileOld;
                return View(model);
            }

            if (model.GroupId == -1){
                ViewBag.Groups = await _GetSelectList();
                TempData["error"] = "Cập nhật thất bại, Chọn nhóm cho món ăn.";
                return View(model);
            }


            model.Name = model.Name.Trim().ToUpper();
            
            var menu = await _context.Menus.Where(m => m.Name == model.Name && m.Id != model.Id).FirstOrDefaultAsync();

            if(menu != null){
                TempData["Error"] = "Cập nhật thất bại, Tên món ăn đã tồn tại.";
                model.ImageSrc = srcFileOld;
                return View(model);
            }

            
            var file = Request.Form.Files.Count() > 0 ? Request.Form.Files["ImageSrc"] : null;
            
            if (file != null){
                string error = CheckFileUpload(file);
                if(!string.IsNullOrEmpty(error)){
                    TempData["Error"] = error;
                    model.ImageSrc = srcFileOld;
                    return View(model);
                }

                string extension = Path.GetExtension(file.FileName).ToLower();
                string fileName = model.Name.Replace(" ", "_") + extension;
                string filePath = Path.Combine("wwwroot", "img", "menu", fileName);
                RemoveFileInFolder(srcFileOld);    // Xoa file anh trong du an
                using(var stream = new FileStream(filePath, FileMode.Create)){ 
                    await file.CopyToAsync(stream);
                    model.ImageSrc = filePath.Substring(7);
                }
            }else{
                _logger.LogWarning("No FIle");
                model.ImageSrc = srcFileOld;
            }

            _context.Menus.Update(model);
            await _context.SaveChangesAsync();
            //ViewBag.Groups = await _GetSelectList();
            TempData["Success"] = "Cập nhật món thành công.";
            return RedirectToAction("Index");
        }

        [Route("/thuc-don/Detail/{id=0}")]
        public async Task<IActionResult> DetailAsync(int id = 0)
        {
            Menu menu = await _context.Menus
                        .Include(m => m.Group)
                        .FirstOrDefaultAsync(m => m.Id == id);

            if (menu == null) {
                TempData["Error"] = "Không tìm thấy món ăn";
                return RedirectToAction("Index");
            }
            return View(menu);
        }
        public async Task<IActionResult> DeleteAsync(int id){
            var findMenu = await _context.Menus.FirstOrDefaultAsync(m => m.Id == id);

            if(findMenu != null){
                RemoveFileInFolder(findMenu.ImageSrc);  // Xóa file
                _context.Menus.Remove(findMenu);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Xóa thành công.";
            return RedirectToAction("Index");
        }
    



        //-----------------------------------------------
        //@* Kiem tra file hop le *@
        public string CheckFileUpload(IFormFile file)
        {
            int maxSize = 1048576*2;
            string[] extensionList = new string[] { ".jpeg", ".png", ".jpg", ".webp" };
            string extension = Path.GetExtension(file.FileName).ToLower();
        
            if (!extensionList.Contains(extension))
            {
                return $"Định dạng ảnh \"{extension}\" không hợp lệ. Yêu cầu ảnh có dạng {string.Join(", ", extensionList)}";
                
            }

            if(file.Length > maxSize)
            {
                return $"Dung lượng file quá lớn (Dung lượng tối đa: 2MB).";
            }


            return "";
        }

        //@* xoa file trong folder *@
        public void RemoveFileInFolder(string? filePath = null)
        {
            if(filePath != null){
                string fileName = Path.GetFileName(filePath);
                string? dir = Path.GetDirectoryName(filePath);
                string folder = Path.Combine("wwwroot" + dir, fileName);
                if (System.IO.File.Exists(folder))
                {
                    System.IO.File.Delete(folder);
                }
            }
        }


        /*
            Lay danh sach nhom 
         */
        async Task<SelectList> _GetSelectList(int? currentId = 0, int? parentId = 0)
        {
            var query = _context.Groups
                        .Include(m => m.Parent)
                        .Include(m => m.ChildrenGroup);
            var groups = (await query.ToListAsync())
                            .Where(m => m.Parent == null)
                            .OrderBy(m => m.Arrange)                
                            .ToList();
            groups.Insert(0, new Group()
            {
                Id = -1,
                Name = "KHÔNG",
            });

            List<Group> result = new List<Group>();
            foreach (var item in groups)
            {
                int level = 0;
                result = await _GetChildren(result, item, level, currentId);
            }

            if (parentId == null || parentId == 0)
            {
                parentId = -1;
            }

            return new SelectList(result, "Id", "Name", parentId);
        }

        async Task<List<Group>> _GetChildren(List<Group>? result, Group item, int level, int? currentId)
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


        // Lấy danh sach group id là con của group đầu vào
        async Task<List<int>> _GetListGroupId(int? id)
        {
            List<int> listId = new List<int>();
            var group = await _context.Groups
                                .Include(m => m.ChildrenGroup)
                                .FirstOrDefaultAsync(m => m.Id == id);

            if (group != null)
            {
                await _SetChildrenGroup(listId, group);
            }
            
            return listId;
        }

        async Task _SetChildrenGroup(List<int> result, Group group)
        {
            result.Add(group.Id);
            if (group.ChildrenGroup != null && group.ChildrenGroup.Count() > 0)
            {
                foreach (var child in group.ChildrenGroup)
                {
                    await _SetChildrenGroup(result, child);
                }
            }
        }
    
    }


}
