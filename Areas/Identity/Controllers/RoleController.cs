// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using App.Areas.Identity.Models.ManageViewModels;
using App.Areas.Identity.Models.RoleViewModels;
using App.Data;
using App.ExtendMethods;
using AppFoods.Models;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppFoods.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Org.BouncyCastle.Crypto.Engines;

namespace App.Areas.Identity.Controllers
{

    [Authorize(Policy = ClaimName.RoleManager)]
    [Area("Identity")]
    [Route("/Role/[action]")]
    public class RoleController : Controller
    {
        
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        public RoleController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /Role/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            
           var r = await _roleManager.Roles.OrderBy(r => r.Name).Where(r => r.Name != RoleName.Administrator).ToListAsync();
           var roles = new List<RoleModel>();
           foreach (var _r in r)
           {
               var claims = await _roleManager.GetClaimsAsync(_r);
               var claimsString = claims.Select(c => c.Value).ToArray();

               var rm = new RoleModel()
               {
                   Name = _r.Name,
                   Id = _r.Id,
                   Claims = claimsString
               };
               roles.Add(rm);
           }

            return View(roles);
        } 

        // GET: /Role/Create
        [HttpGet]
        public IActionResult Create()
        {
            //ViewBag.Claims = new SelectList(ClaimName.ListClaims());
            ViewBag.Claims = new SelectList(ClaimName.DirClaims().Keys);
            return View();
        }
        
        // POST: /Role/Create
        [HttpPost, ActionName(nameof(Create))]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(CreateRoleModel model)
        {
            var dir = ClaimName.DirClaims();
            ViewBag.Claims = new SelectList(dir.Keys);
            if  (!ModelState.IsValid)
            {
                return View();
            }

            var role = await _roleManager.FindByNameAsync(model.Name);
            if(role != null)
            {
                TempData["Error"] = "Role đã tồn tại";
                return View(model);
            }
            var newRole = new IdentityRole(model.Name);
            var result = await _roleManager.CreateAsync(newRole);
            if (result.Succeeded)
            {
                foreach(var claim in model.Claims)
                {
                    await _roleManager.AddClaimAsync(newRole, new Claim(dir[claim], claim));
                }
                TempData["Success"] = $"Bạn vừa tạo role mới: {model.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View(model);
        }     

        // GET: /Role/Delete/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> DeleteAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 
            return View(role);
        }
        
        // POST: /Role/Edit/1
        [HttpPost("{roleid}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if  (role == null) return NotFound("Không tìm thấy role");
             
            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded)
            {
                StatusMessage = $"Bạn vừa xóa: {role.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }
            return View(role);
        }     

        // GET: /Role/Edit/roleid
        [HttpGet("{roleid}")]
        public async Task<IActionResult> EditAsync(string roleid, [Bind("Name")]EditRoleModel model)
        {
            ViewBag.Claims = new SelectList(ClaimName.DirClaims().Keys);
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 
            model.Name = role.Name;
            model.Claims = await _context.RoleClaims
                            .Where(rc => rc.RoleId == role.Id)
                            .Select(rc => rc.ClaimType)
                            .ToArrayAsync();

            model.role = role;
            ModelState.Clear();
            return View(model);

        }
        
        // POST: /Role/Edit/1
        [HttpPost("{roleid}"), ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConfirmAsync(string roleid, [Bind("Name","Claims")]EditRoleModel model)
        {
            var dir = ClaimName.DirClaims();
            ViewBag.Claims = new SelectList(dir.Keys);

            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            }

            var check = await _context.Roles.FirstOrDefaultAsync(r => r.Name == model.Name && r.Id != roleid);
            if(check != null)
            {
                TempData["Error"] = "Role đã tồn tại";
                return View(model);
            }
            model.role = role;
    
            role.Name = model.Name;
            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded)
            {
                await _context.RoleClaims.Where(rc => rc.RoleId == roleid).ExecuteDeleteAsync();

                foreach (var claim in model.Claims)
                {
                    await _roleManager.AddClaimAsync(role, new Claim(dir[claim], claim));
                }

                TempData["Success"] = $"Bạn đã cập nhật role: {model.Name}";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(result);
            }

            return View(model);
        }

        // GET: /Role/AddRoleClaim/roleid
        [HttpGet("{roleid}")]        
        public async Task<IActionResult> AddRoleClaimAsync(string roleid)
        {
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 

            var model = new EditClaimModel()
            {
                role = role
            };
            return View(model);
        }             

        // POST: /Role/AddRoleClaim/roleid
        [HttpPost("{roleid}")]  
        [ValidateAntiForgeryToken]      
        public async Task<IActionResult> AddRoleClaimAsync(string roleid, [Bind("ClaimType", "ClaimValue")]EditClaimModel model)
        {
            
            if (roleid == null) return NotFound("Không tìm thấy role");
            var role = await _roleManager.FindByIdAsync(roleid);
            if (role == null)
            {
                return NotFound("Không tìm thấy role");
            } 

            
            model.role = role;

            if ((await _roleManager.GetClaimsAsync(role)).Any(c => c.Type == model.ClaimType && c.Value == model.ClaimValue))
            {
                TempData["Error"] = "Claim này đã có trong role";
                return View(model);
            }

            var newClaim = new Claim(model.ClaimType, model.ClaimValue);
            var result = await _roleManager.AddClaimAsync(role, newClaim);
            
            if (!result.Succeeded)
            {
                ModelState.AddModelError(result);
                return View(model);
            }
            
            TempData["Success"] = "Vừa thêm đặc tính (claim) mới";
            
            return RedirectToAction("Edit", new {roleid = role.Id});

        }          

        // GET: /Role/EditRoleClaim/claimid
        [HttpGet("{claimid:int}")]        
        public async Task<IActionResult> EditRoleClaim(int claimid)
        {
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy role");
            ViewBag.claimid = claimid;

            var Input = new EditClaimModel()
            {
                ClaimType = claim.ClaimType,
                ClaimValue = claim.ClaimValue,
                role = role
            };


            return View(Input);
        }             

        // GET: /Role/EditRoleClaim/claimid
        [HttpPost("{claimid:int}")]        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRoleClaim(int claimid, [Bind("ClaimType", "ClaimValue")]EditClaimModel Input)
        {
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy role");

            ViewBag.claimid = claimid;

            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy role");

            Input.role = role;
            if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != claim.Id))
            {
                TempData["Error"] = "Claim này đã có trong role";
                return View(Input);
            }
 

            claim.ClaimType = Input.ClaimType;
            claim.ClaimValue = Input.ClaimValue;
            
            await _context.SaveChangesAsync();
            
            TempData["Success"] = "Vừa cập nhật claim";
            
            return RedirectToAction("Edit", new {roleid = role.Id});
        }        
        // POST: /Role/EditRoleClaim/claimid
        [HttpPost("{claimid:int}")]        
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaim(int claimid, [Bind("ClaimType", "ClaimValue")]EditClaimModel Input)
        {
            var claim = _context.RoleClaims.Where(c => c.Id == claimid).FirstOrDefault();
            if (claim == null) return NotFound("Không tìm thấy role");

            var role = await _roleManager.FindByIdAsync(claim.RoleId);
            if (role == null) return NotFound("Không tìm thấy role");
            Input.role = role;
            
            if (_context.RoleClaims.Any(c => c.RoleId == role.Id && c.ClaimType == Input.ClaimType && c.ClaimValue == Input.ClaimValue && c.Id != claim.Id))
            {
                ModelState.AddModelError(string.Empty, "Claim này đã có trong role");
                return View(Input);
            }
 

            await _roleManager.RemoveClaimAsync(role, new Claim(claim.ClaimType, claim.ClaimValue));
            
            StatusMessage = "Vừa xóa claim";

            
            return RedirectToAction("Edit", new {roleid = role.Id});
        }        


    }
}
