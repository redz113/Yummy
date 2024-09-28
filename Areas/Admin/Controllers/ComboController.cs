using AppFoods.Data;
using AppFoods.Models;
using AppFoods.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol;

namespace AppFoods.Areas.Admin.Controllers
{
    [Authorize(Policy = ClaimName.MenuManager)]
    [Area("Admin")]
    [Route("/Combo/[Action]/{id?}")]
    public class ComboController : Controller, IMyController
    {
        private readonly AppDbContext _context;

        public ComboController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var combos = await _context.Combos.ToListAsync();
            ViewBag.Combos = combos;
            return View();
        }

        public async Task<IActionResult> CreateAsync()
        {
            var query = _context.Groups
                        .Include(m => m.Parent)
                        .Include(m => m.ChildrenGroup);
            var groups = (await query.ToListAsync())
                            .Where(m => m.Parent == null)
                            .OrderBy(m => m.Arrange)
                            .ToList();
            ViewBag.ListGroup = groups;
            ViewBag.ListMenu = await _context.Menus.ToListAsync();
            
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(Combo model)     // Model: Combo
        {
            var query = _context.Groups
                        .Include(m => m.Parent)
                        .Include(m => m.ChildrenGroup);
            ViewBag.ListGroup = (await query.ToListAsync())
                            .Where(m => m.Parent == null)
                            .OrderBy(m => m.Arrange)
                            .ToList();
            ViewBag.ListMenu = await _context.Menus.ToListAsync();

            if(!ModelState.IsValid){
                //TempData["error"] = "";
                return View(model);
            }

            var menuId = await _context.Menus.Select(m => m.Id).ToListAsync();
            List<int> ckb = new List<int>();
            foreach(int i in menuId){
                string request = Request.Form[i.ToString()];
                if(request != null){
                    ckb.Add(i);
                }
            }

            if(ckb.Count > 0){
                model.Menu = string.Join(",", ckb);
            }else{
                TempData["error"] = "Chọn món cho combo";
                return View(model);
            }

            model.Name = model.Name.ToUpper().Trim();
            var combo = await _context.Combos.FirstOrDefaultAsync(c => c.Name == model.Name);
            if(combo != null){
                TempData["error"] = "Tên combo đã tồn tại";
                return View(model);
            }

            var file = Request.Form.Files.Count() > 0 ? Request.Form.Files["ImageSrc"] : null;
            if(file != null){
                string error = CheckFileUpload(file);
                if(!string.IsNullOrEmpty(error)){
                    TempData["Error"] = error;
                    return View(model);
                }

                string extension = Path.GetExtension(file.FileName).ToLower();
                string fileName = model.Name.Replace(" ", "_") + extension;
                string filePath = Path.Combine("wwwroot", "img", "combo", fileName);
                using(var stream = new FileStream(filePath, FileMode.Create)){
                    await file.CopyToAsync(stream);
                    model.ImageSrc = filePath.Substring(7);
                }
            }
            
            await _context.Combos.AddAsync(model);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã thêm 1 combo mới";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> DeleteAsync(int id)
        {
            var combo = await _context.Combos.FirstOrDefaultAsync(c => c.Id == id);
            if(combo != null){
                RemoveFileInFolder(combo.ImageSrc);
                _context.Combos.Remove(combo);
                await _context.SaveChangesAsync();
            }
            TempData["success"] = "Đã xóa";
            return RedirectToAction("Index");
        }

        [Route("/combo/Edit/{id=0}")]
        public async Task<IActionResult> EditAsync(int id)
        {
            var combo = await _context.Combos.FirstOrDefaultAsync(c => c.Id == id);
            if(combo == null){
                TempData["error"] = "Không tìm thấy combo";
                return RedirectToAction("Index");
            }
           var query = _context.Groups
                        .Include(m => m.Parent)
                        .Include(m => m.ChildrenGroup);
            var groups = (await query.ToListAsync())
                            .Where(m => m.Parent == null)
                            .OrderBy(m => m.Arrange)
                            .ToList();
            ViewBag.ListGroup = groups;
            ViewBag.ListMenu = await _context.Menus.ToListAsync();
            
            return View(combo);
        }

        [HttpPost]
        [Route("/combo/Edit/{id=0}")]
        public async Task<IActionResult> EditAsync(int id, Combo model)
        {
            var query = _context.Groups
                        .Include(m => m.Parent)
                        .Include(m => m.ChildrenGroup);
            ViewBag.ListGroup = (await query.ToListAsync())
                            .Where(m => m.Parent == null)
                            .OrderBy(m => m.Arrange)
                            .ToList();
            ViewBag.ListMenu = await _context.Menus.ToListAsync();

            if(!ModelState.IsValid){
                //TempData["error"] = "";
                return View(model);
            }

            var menuId = await _context.Menus.Select(m => m.Id).ToListAsync();
            List<int> ckb = new List<int>();
            foreach(int i in menuId){
                string request = Request.Form[i.ToString()];
                if(request != null){
                    ckb.Add(i);
                }
            }

            if(ckb.Count > 0){
                model.Menu = string.Join(",", ckb);
            }else{
                TempData["error"] = "Chọn món cho combo";
                return View(model);
            }
            model.Name = model.Name.ToUpper().Trim();
            var combo = await _context.Combos.FirstOrDefaultAsync(c => c.Name == model.Name && c.Id != model.Id);
            if(combo != null){
                TempData["error"] = "Tên combo đã tồn tại";
                return View(model);
            }

            var file = Request.Form.Files.Count() > 0 ? Request.Form.Files["UploadFile"] : null;
            if(file != null){
                string error = CheckFileUpload(file);
                if(!string.IsNullOrEmpty(error)){
                    TempData["Error"] = error;
                    return View(model);
                }

                string extension = Path.GetExtension(file.FileName).ToLower();
                string fileName = model.Name.Replace(" ", "_") + extension;
                string filePath = Path.Combine("wwwroot", "img", "combo", fileName);
                RemoveFileInFolder(model.ImageSrc);
                using(var stream = new FileStream(filePath, FileMode.Create)){
                    await file.CopyToAsync(stream);
                    model.ImageSrc = filePath.Substring(7);
                }
            }

            _context.Combos.Update(model);
            await _context.SaveChangesAsync();
            TempData["success"] = "Đã lưu thay đổi";
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

        public async Task<IActionResult> DetailAsync(int id)
        {
            var combo = await _context.Combos.FirstOrDefaultAsync(c => c.Id == id);
            if(combo == null){
                return NotFound();
            }
            string menu = combo.Menu;
            List<string> menuId = menu.Split(",").ToList();
            
            var menus = await _context.Menus
                            .Where(m => menuId.Contains(m.Id.ToString()))
                            // .OrderBy(m => m.Group.Arrange)
                            .Select(m => m.Name)
                            .ToListAsync();
            ViewBag.Menus = menus;
            return View(combo);
        }
    }
}
