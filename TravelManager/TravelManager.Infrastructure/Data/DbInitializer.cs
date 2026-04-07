using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TravelManager.Domain.Entities;

namespace TravelManager.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAdminAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Email нашого головного адміна
            var adminEmail = "admin@travel.com";

            // Перевіряємо, чи він вже існує
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new User
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FirstName = "System",
                    LastName = "Administrator",
                    CreatedAt = DateTime.UtcNow
                };

                // Створюємо адміна з паролем
                var result = await userManager.CreateAsync(admin, "AdminPass123!");

                if (result.Succeeded)
                {
                    // Призначаємо йому роль Admin, яку ми вже засіяли в DatabaseSeeder
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}
