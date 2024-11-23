using EventSystem.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventSystem.Data
{
    public class DbInitializer
    {
        public static async Task Initialize(IServiceProvider serviceProvider, ApplicationDbContext context)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

            //Създаваме админ роля, ако тя не съществува
            var roleExist = await roleManager.RoleExistsAsync("Admin");
            if (!roleExist)
            {
                var adminRole = new IdentityRole<Guid>("Admin");
                await roleManager.CreateAsync(adminRole);
            }

            //Създаваме админ User, ако той не съществува
            var adminUser = await userManager.FindByEmailAsync("AdminEmail");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "AdminUsername",
                    Email = "AdminEmail",
                    FirstName = "Admin",
                    LastName = "User"
                };
                
                var createAdminResult = await userManager.CreateAsync(adminUser, "Admin123"); //Парола
                if (createAdminResult.Succeeded)
                {
                    //Прилагаме админ ролята на user-a
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }
    }
}
