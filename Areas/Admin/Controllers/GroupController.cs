using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AppFoods.Models;
using Bogus;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using AppFoods.Data;
using NuGet.Protocol;

namespace AppFoods.Areas_Admin_Controllers
{
    [Authorize(Policy = ClaimName.GroupManager)]
    [Area("Admin")]
    [Route("/nhom/[Action]/")]
    public class GroupController : Controller
    {
        private readonly AppDbContext _context;

        public GroupController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Group
        public async Task<IActionResult> Index()
        {
            var groups = await _GetListGroups();
            return View(groups);
        }

        // GET: Group
        public async Task<IActionResult> Arrange()
        {
            ViewBag.Groups = await _GetListGroups();
            
            ViewBag.SelectListGroups =  new SelectList(ViewBag.Groups, "Id", "Name");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ArrangeAsync()
        {
            var vt1 = Int32.Parse(Request.Form["vt1"]);
            var vt2 = Int32.Parse(Request.Form["vt2"]);

            if(vt1 != vt2 && vt1 != -1 && vt2 != -1){
                var groups = await _context.Groups.Where(m => m.Id == vt1 || m.Id == vt2).ToListAsync();
                if(groups.Count() == 2){
                    var flag = groups[0].Arrange;
                    groups[0].Arrange = groups[1].Arrange;
                    groups[1].Arrange = flag;

                    _context.Groups.UpdateRange(groups);
                    await _context.SaveChangesAsync();
                }
            }

            ViewBag.Groups = await _GetListGroups();
            ViewBag.SelectListGroups =  new SelectList(ViewBag.Groups, "Id", "Name");
            return View();
        }

        // GET: Group/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(m => m.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // GET: Group/Create
        public async Task<IActionResult> Create()
        {
            ViewBag.ParentId = await _GetSelectList();
            /*ViewData["ParentId"] = new SelectList(_context.Groups, "Id", "Name");*/
            return View();
        }

       

        // POST: Group/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParentId")] Group @group)
        {
            if (ModelState.IsValid)
            {
                if (@group.ParentId == -1) @group.ParentId = null;
                @group.Name  = @group.Name.ToUpper().Trim();

                var check = await _context.Groups.FirstOrDefaultAsync(g => g.Name == @group.Name && g.Id != @group.Id);

                if (check == null)
                {
                    int arrange = await _context.Groups.CountAsync();
                    @group.Arrange = arrange + 1;
                    _context.Add(@group);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Đã thêm nhóm mới";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    TempData["error"] = "Tên nhóm đã tồn tại";
                }            
            }
            ViewBag.ParentId = await _GetSelectList();
            return View(@group);
        }

        // GET: Group/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups.FindAsync(id);
            if (@group == null)
            {
                return NotFound();
            }
            ViewBag.ParentId = await _GetSelectList(id, @group.ParentId);
            return View(@group);
        }

        // POST: Group/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,ParentId,Arrange")] Group @group)
        {
            if (id != @group.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (@group.ParentId == -1) @group.ParentId = null;
                    @group.Name = @group.Name.ToUpper().Trim();
                    _context.Update(@group);
                    await _context.SaveChangesAsync();
                    TempData["success"] = "Đã cập nhật thành công";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GroupExists(@group.Id))
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
            ViewBag.ParentId = await _GetSelectList(id, @group.ParentId);
            return View(@group);
        }

        // GET: Group/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @group = await _context.Groups
                .Include(m => m.Parent)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (@group == null)
            {
                return NotFound();
            }

            return View(@group);
        }

        // POST: Group/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @group = await _context.Groups
                            .Include(m => m.ChildrenGroup)
                            .FirstOrDefaultAsync(m => m.Id == id);
            
            var n = await _context.Menus.Where(m => m.GroupId == id).CountAsync();

            if(n > 0){
                TempData["error"] = "Xóa thất bại. Không thể xóa khi trong nhóm còn món ăn.";
                return RedirectToAction(nameof(Index));
            }

            if (@group != null)
            {

                if (@group.ChildrenGroup?.Count() > 0) 
                {
                    foreach (var c in @group.ChildrenGroup)
                    {
                        c.ParentId = @group.ParentId;
                        _context.Groups.Update(c);
                    }
                }
                _context.Groups.Remove(@group);
            }

            TempData["success"] = "Xóa thành công";
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GroupExists(int id)
        {
            return _context.Groups.Any(e => e.Id == id);
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
    
    }
}
