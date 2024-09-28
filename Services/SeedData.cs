using App.Data;
using AppFoods.Data;
using AppFoods.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Net;

namespace AppFoods.Services
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceScope scope)
        {
            var _roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var _userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            /// Tao cac Role mac dinh cho he thong
            var roleNames = typeof(RoleName).GetFields().ToList();      // lấy các trường dữ liệu của lớp RoleName
      
            foreach (var role in roleNames) {
                string roleName = (string) role.GetRawConstantValue();  // Trả về giá trị của trường dữ liệu
                var check = await _roleManager.FindByNameAsync(roleName);

                if (check == null) {
                    await _roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            /// Tao tai khoan Admin mac dinh
            /// Name:       administrator
            /// UserName:   administrator
            /// Email:   administrator
            /// Password:   1
            /// 

            var user = await _userManager.FindByEmailAsync("adminitrator@example.com");
            if (user == null)
            {
                var userAdmin = new AppUser()
                {
                    Name = "Administrator",
                    UserName = "administrator",
                    Email = "adminitrator@example.com",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(userAdmin, "administrator123");
                await _userManager.AddToRoleAsync(userAdmin, RoleName.Administrator);
            }

            var manager = await _userManager.FindByEmailAsync("manager@example.com");
            if (manager == null)
            {
                var userManager = new AppUser()
                {
                    Name = "Manager",
                    UserName = "manager",
                    Email = "manager@example.com",
                    EmailConfirmed = true
                };

                await _userManager.CreateAsync(userManager, "manager123");
                await _userManager.AddToRoleAsync(userManager, RoleName.Manager);
            }

        }

        public static async Task AuthorizationAddPolicy(AuthorizationOptions opts)
        {
            var claims = ClaimName.DirClaims().Keys;
            foreach(var claim in claims)
            {
                opts.AddPolicy(claim, policy => {
                    policy.RequireAssertion(context =>
                    {
                        return context.User.IsInRole(RoleName.Administrator)
                        || context.User.Claims.Any(c => c.Value == claim)
                        ;
                    });
                });
            }
        }
    }
}
