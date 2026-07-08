using Microsoft.AspNetCore.Identity;

namespace MealBuilder.Web.Identity
{
    public static class InitialAdminSeeder
    {
        public static async Task SeedAsync(
            IServiceProvider serviceProvider,
            IConfiguration configuration)
        {
            using IServiceScope scope = serviceProvider.CreateScope();

            RoleManager<IdentityRole> roleManager =
                scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            UserManager<ApplicationUser> userManager =
                scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            string? email = configuration["InitialAdmin:Email"];
            string? password = configuration["InitialAdmin:Password"];

            if (string.IsNullOrWhiteSpace(email))
            {
                throw new InvalidOperationException(
                    "InitialAdmin:Email is not configured.");
            }

            ApplicationUser? adminUser =
                await userManager.FindByEmailAsync(email);

            if (adminUser is null && string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "InitialAdmin:Password is not configured.");
            }

            if (!await roleManager.RoleExistsAsync(ApplicationRoles.Admin))
            {
                IdentityResult roleResult =
                    await roleManager.CreateAsync(
                        new IdentityRole(ApplicationRoles.Admin));

                EnsureSucceeded(roleResult, "Admin role creation");
            }

            if (adminUser is null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                IdentityResult userResult =
                    await userManager.CreateAsync(adminUser, password!);

                EnsureSucceeded(userResult, "Initial Admin creation");
            }

            if (!await userManager.IsInRoleAsync(
                    adminUser,
                    ApplicationRoles.Admin))
            {
                IdentityResult roleAssignmentResult =
                    await userManager.AddToRoleAsync(
                        adminUser,
                        ApplicationRoles.Admin);

                EnsureSucceeded(
                    roleAssignmentResult,
                    "Admin role assignment");
            }
        }

        private static void EnsureSucceeded(
            IdentityResult result,
            string operation)
        {
            if (result.Succeeded)
            {
                return;
            }

            string errors = string.Join(
                "; ",
                result.Errors.Select(error => error.Description));

            throw new InvalidOperationException(
                $"{operation} failed: {errors}");
        }
    }
}
