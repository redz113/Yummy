// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using App.Areas.Identity.Models.AccountViewModels;
using App.Areas.Identity.Models.ManageViewModels;
using App.Areas.Identity.Models.RoleViewModels;
using App.Areas.Identity.Models.UserViewModels;
using App.Data;
using App.ExtendMethods;
using AppFoods.Models;
using App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AppFoods.Areas.Identity.Models.User;
using AppFoods.Data;
using System.Data;

namespace App.Areas.Identity.Controllers
{

    //[Authorize(Roles = RoleName.Administrator)]
    [Authorize(Policy = ClaimName.UserManager)]
    [Area("Identity")]
    [Route("/ManageUser/[action]")]
    public class UserController : Controller
    {
        
        private readonly ILogger<RoleController> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;

        public UserController(ILogger<RoleController> logger, RoleManager<IdentityRole> roleManager, AppDbContext context, UserManager<AppUser> userManager)
        {
            _logger = logger;
            _roleManager = roleManager;
            _context = context;
            _userManager = userManager;
        }



        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /ManageUser/Index
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage)
        {
            var model = new UserListModel();
            model.currentPage = currentPage;

            var qr = _userManager.Users.OrderBy(u => u.UserName);

            model.totalUsers = await qr.CountAsync();
            model.countPages = (int)Math.Ceiling((double)model.totalUsers / model.ITEMS_PER_PAGE);

            if (model.currentPage < 1)
                model.currentPage = 1;
            if (model.currentPage > model.countPages)
                model.currentPage = model.countPages;

            var qr1 = qr.Skip((model.currentPage - 1) * model.ITEMS_PER_PAGE)
                        .Take(model.ITEMS_PER_PAGE)
                        .Select(u => new UserAndRole() {
                            Id = u.Id,
                            UserName = u.UserName,
                            Name = u.Name
                        });

            model.users = await qr1.ToListAsync();

            foreach (var user in model.users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                user.RoleNames = string.Join(",", roles);
            } 
            
            return View(model);
        } 

        // GET: /ManageUser/AddRole/id
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {

            // public SelectList allRoles { get; set; }
            var model = new AddUserRoleModel();
            if (string.IsNullOrEmpty(id))
            {
                return Content("1");
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return Content("2");

                return NotFound($"Không thấy user, id = {id}.");
            }

            //model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToList();

            await GetClaims(model);

            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, [Bind("RoleChecks")] AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }
            
            var roleNameNew = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            for(int i = roleNameNew.Count - 1; i >= 0; i--)
            {
                if (!model.RoleChecks[i])
                {
                    roleNameNew.RemoveAt(i);
                }
            }

            await GetClaims(model);

            var OldRoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();

            var deleteRoles = OldRoleNames.Where(r => !roleNameNew.Contains(r));
            var addRoles = roleNameNew.Where(r => !OldRoleNames.Contains(r));

                      

            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user,deleteRoles);
            if (!resultDelete.Succeeded)
            {
                //return Content("1");
                ModelState.AddModelError(resultDelete);
                return View(model);
            }
            
            var resultAdd = await _userManager.AddToRolesAsync(model.user,addRoles);
            if (!resultAdd.Succeeded)
            {
                return Content("2");

                ModelState.AddModelError(resultAdd);
                return View(model);
            }


            TempData["Success"] = $"Đã cập nhật role cho user: {model.user.UserName}";

            return RedirectToAction("Index");
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> SetPasswordAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            return View();
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPasswordAsync(string id, SetUserPasswordModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }
             
            await _userManager.RemovePasswordAsync(user);

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            StatusMessage = $"Vừa cập nhật mật khẩu cho user: {user.UserName}";

            return RedirectToAction("Index");
        }        


        [HttpGet("{userid}")]
        [Authorize(Policy = (ClaimName.RoleManager))]
        public async Task<ActionResult> AddClaimAsync(string userid, AddUserClaimModel model)
        {
            // ViewBag.Claims = ClaimName.DirClaims();
            var user = await _userManager.FindByIdAsync(userid);

            ViewBag.Claims = GetAddUserClaims(userid);


            if (user == null) return NotFound("Không tìm thấy user");
            ViewBag.user = user;
            return View(model);
        }

        [HttpPost("{userid}")]
        [ValidateAntiForgeryToken]
        [Authorize(Policy = (ClaimName.RoleManager))]
        public async Task<ActionResult> AddClaim(string userid, AddUserClaimModel model)
        {
            var claims = GetAddUserClaims(userid);
            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return NotFound("Không tìm thấy user");

            ViewBag.user = user;

            var claim = await _userManager.GetClaimsAsync(user);
            foreach (var c in claim)
            {
                await _userManager.RemoveClaimAsync(user, c);
            }
            
            for(int i=0; i<model.Claims.Count; i++)
            {
                if (model.Claims[i] == true)
                {
                    var keys = claims.Keys.ToArray();
                    var value = claims[keys[i]];

                    Claim addClaim = new Claim(value, keys[i]);
                    await _userManager.AddClaimAsync(user, addClaim);
                }
            }

            TempData["Success"] = "Đã thêm đặc tính cho user";
                        
            return RedirectToAction("AddRole", new {id = user.Id});
        }        

        [HttpPost("{claimid}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteClaimAsync(int claimid)
        {
            var userclaim = _context.UserClaims.Where(c => c.Id == claimid).FirstOrDefault();
            var user = await _userManager.FindByIdAsync(userclaim.UserId);

            if (user == null) return NotFound("Không tìm thấy user");

            await _userManager.RemoveClaimAsync(user, new Claim(userclaim.ClaimType, userclaim.ClaimValue));

            StatusMessage = "Bạn đã xóa claim";
            
            return RedirectToAction("AddRole", new {id = user.Id});
        }

        private async Task GetClaims(AddUserRoleModel model)
        {
            var listRolesInUser = from r in _context.Roles
                join ur in _context.UserRoles on r.Id equals ur.RoleId
                where ur.UserId == model.user.Id
                select r;

            var _claimsInRole  = await (from c in _context.RoleClaims
                                 join r in listRolesInUser on c.RoleId  equals r.Id
                                 select c).ToListAsync();


            int count = _claimsInRole.Count();
            HashSet<int> remove = new HashSet<int>();
            for (int i = 0; i < count-1; i++)
            {
                for (int j = i+1; j < count; j++)
                {
                    if (_claimsInRole[i].ClaimType == _claimsInRole[j].ClaimType)
                    {
                        remove.Add(j);
                    }
                }
            }

            remove.Reverse();
            foreach(int i in remove)
            {
                _claimsInRole.RemoveAt(i);
            }

            model.claimsInRole = _claimsInRole;
            

           model.claimsInUserClaim  = await (from c in _context.UserClaims
            where c.UserId == model.user.Id select c).ToListAsync();

            List<string> listRoles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            if (!User.IsInRole(RoleName.Administrator))
            {
                listRoles.Remove(RoleName.Administrator);
            }
            ViewBag.allRoles = listRoles;

            var roleNames = (await _userManager.GetRolesAsync(model.user)).ToList();
            
            List<bool> roleChecks = new List<bool>();

            foreach (var role in listRoles)
            {
                if (roleNames.Contains(role))
                {
                    roleChecks.Add(true);
                }
                else
                {
                    roleChecks.Add(false);
                }
            }

            model.RoleChecks = roleChecks;
        }

        [HttpGet]
        public async Task<IActionResult> AddUser()
        {
            await SetModelAddUserAsync(new AddUserModel());
            return View();
        }

        private async Task SetModelAddUserAsync(AddUserModel model)
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToListAsync();
            if (!User.IsInRole(RoleName.Administrator))
            {
                roles.Remove(RoleName.Administrator);
            }

            ViewBag.allRoles = roles;
            if (model.RoleChecks == null)
            {
                var checkRoles = new List<bool>();
               foreach(var r in roles)
                {
                    checkRoles.Add(false);
                }
               model.RoleChecks = checkRoles;
            }

        }

        [HttpPost]
        public async Task<IActionResult> AddUserAsync(AddUserModel model)
        {
            await SetModelAddUserAsync(model);
            if (!ModelState.IsValid) {
                TempData["Error"] = "Phải nhập đầy đủ thông tin";
                
                return View(model);
            };

            AppUser user = new AppUser()
            {
                Name = model.Name,
                UserName = Guid.NewGuid().ToString(),
                Email = model.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                List<string> addRole = ViewBag.allRoles;
                int count = addRole.Count;
                for (int i = count-1; i >= 0; i--) {
                    if (!model.RoleChecks[i])
                    {
                        addRole.RemoveAt(i);
                    }
                }

                await _userManager.AddToRolesAsync(user, addRole);
                TempData["Success"] = "Thêm mới thành công";
            }else{
                TempData["error"] = "Email đã được đăng ký với một tài khoản có trên hệ thống.";
                return View(model);
            }   

            
            return RedirectToAction(nameof(Index));
        }

        // Cá claim có thể thêm
        private Dictionary<string, string> GetAddUserClaims(string userId)
        {

            var ListRoles = from r in _context.Roles 
                            join ur in _context.UserRoles on r.Id equals ur.RoleId
                            where ur.UserId == userId
                            select r;
            var roleClaims = (from rc in _context.RoleClaims 
                             join r in ListRoles on rc.RoleId equals r.Id
                             select rc).ToList();                 // Distinct - Bỏ các giá trị trùng

            var listClaims = ClaimName.DirClaims();

            var userClaims = (from uc in _context.UserClaims
                              where uc.UserId == userId
                              select uc).ToList();

           
            foreach (var claim in listClaims) {
                foreach (var roleClaim in roleClaims)
                {
                    if(claim.Value == roleClaim.ClaimType)
                    {
                        listClaims.Remove(claim.Key); continue;
                    }
                } 
            }

            return listClaims;
        }
    }
}
