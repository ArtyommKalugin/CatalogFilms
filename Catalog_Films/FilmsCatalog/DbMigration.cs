using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FilmsCatalog.Data;
using FilmsCatalog.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FilmsCatalog
{
    public static class DbMigration
    {
        public static IWebHost MigrateDatabase(this IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();
                DbMigration.ConfigureIdentity(scope).GetAwaiter().GetResult();
            }

            return webHost;
        }

        private static async Task ConfigureIdentity(IServiceScope scope)
        {
            var roleManager = scope.ServiceProvider.GetService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetService<UserManager<User>>();

            var adminsRole = await roleManager.FindByNameAsync(ApplicationRoles.Administrators);
            if (adminsRole == null)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Administrators));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create {ApplicationRoles.Administrators} role.");
                }

                adminsRole = await roleManager.FindByNameAsync(ApplicationRoles.Administrators);
            }

            var adminUser = await userManager.FindByNameAsync("admin@localhost.local");
            if (adminUser == null)
            {
                var userResult = await userManager.CreateAsync(new User
                {
                    UserName = "admin@localhost.local",
                    Email = "admin@localhost.local"
                }, "AdminPass123!");
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create admin@localhost.local user");
                }

                adminUser = await userManager.FindByNameAsync("admin@localhost.local");
            }

            if (!await userManager.IsInRoleAsync(adminUser, adminsRole.Name))
            {
                await userManager.AddToRoleAsync(adminUser, adminsRole.Name);
            }

            var moderatorsRole = await roleManager.FindByNameAsync(ApplicationRoles.Moderators);
            if (moderatorsRole == null)
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole(ApplicationRoles.Moderators));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create {ApplicationRoles.Moderators} role.");
                }

                moderatorsRole = await roleManager.FindByNameAsync(ApplicationRoles.Moderators);
            }

            var moderatorUser = await userManager.FindByNameAsync("moder@localhost.local");
            if (adminUser == null)
            {
                var userResult = await userManager.CreateAsync(new User
                {
                    UserName = "moder@localhost.local",
                    Email = "moder@localhost.local"
                }, "ModerPass123!");
                if (!userResult.Succeeded)
                {
                    throw new InvalidOperationException($"Unable to create moder@localhost.local user");
                }

                moderatorUser = await userManager.FindByNameAsync("moder@localhost.local");
            }

            if (!await userManager.IsInRoleAsync(moderatorUser, moderatorsRole.Name))
            {
                await userManager.AddToRoleAsync(moderatorUser, moderatorsRole.Name);
            }
        }
    }
}
