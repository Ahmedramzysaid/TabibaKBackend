using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Seeding;

public static class ApplicationUserSeeder
{
    public static async Task SeedAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        const string adminEmail = "admin@example.com";
        const string adminPhone = "1234567890";
        const string adminPassword = "StrongPassword123###";
        
        var existingUserByEmail = await userManager.FindByEmailAsync(adminEmail);
        var existingUserByPhone = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == adminPhone);
        var existingUser = existingUserByEmail ?? existingUserByPhone;
        
        if (existingUser != null)
        {
            if (existingUser.Email != adminEmail || existingUser.PhoneNumber != adminPhone)
            {
                await userManager.DeleteAsync(existingUser);
                existingUser = null;
            }
            else
            {
                await userManager.DeleteAsync(existingUser);
                existingUser = null;
            }
        }

        if (existingUser == null)
        {
            var adminUser = new ApplicationUser
            {
                FullName = "Admin User",
                UserName = adminPhone,
                Email = adminEmail,
                EmailConfirmed = true,
                DateOfBirth = new DateTime(1980, 1, 1),
                Gender = "Male",
                DateOfRegistration = DateTime.UtcNow,
                Latitude = 30.0444,
                Longitude = 31.2357,
                PhoneNumber = adminPhone
            };

            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                Console.WriteLine($"✅ Admin user created successfully: {adminEmail}");
                
                var reloadedUser = await userManager.FindByIdAsync(adminUser.Id);
                if (reloadedUser != null)
                {
                    var passwordCheck = await userManager.CheckPasswordAsync(reloadedUser, adminPassword);
                    if (!passwordCheck)
                    {
                        await userManager.RemovePasswordAsync(reloadedUser);
                        var addPasswordResult = await userManager.AddPasswordAsync(reloadedUser, adminPassword);
                        if (!addPasswordResult.Succeeded)
                        {
                            var token = await userManager.GeneratePasswordResetTokenAsync(reloadedUser);
                            await userManager.ResetPasswordAsync(reloadedUser, token, adminPassword);
                        }
                        Console.WriteLine($"✅ Admin password reset successfully");
                    }
                    
                    var roleId = "d1f488a3-6730-47cb-a0e1-aaa2342a1bc1";
                    var adminRole = await roleManager.FindByIdAsync(roleId) ?? await roleManager.FindByNameAsync("SuperAdmin");
                    if (adminRole != null)
                    {
                        var userRoles = await userManager.GetRolesAsync(reloadedUser);
                        if (!userRoles.Contains(adminRole.Name))
                        {
                            var roleResult = await userManager.AddToRoleAsync(reloadedUser, adminRole.Name);
                            if (roleResult.Succeeded)
                            {
                                Console.WriteLine($"✅ Admin user assigned to SuperAdmin role");
                            }
                            else
                            {
                                Console.WriteLine($"❌ Failed to assign role: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                            }
                        }
                        else
                        {
                            Console.WriteLine($"✅ Admin user already in SuperAdmin role");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"❌ SuperAdmin role not found! Make sure PermissionSeeder runs first.");
                    }
                }
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                Console.WriteLine($"❌ Failed to create admin user: {errors}");
                System.Diagnostics.Debug.WriteLine($"Failed to create admin user: {errors}");
            }
        }
        else
        {
            var existingAdmin = await userManager.FindByEmailAsync(adminEmail) ?? 
                               await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == adminPhone);
            
            if (existingAdmin != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(existingAdmin);
                await userManager.ResetPasswordAsync(existingAdmin, token, adminPassword);
                
                var adminRole = await roleManager.FindByIdAsync("d1f488a3-6730-47cb-a0e1-aaa2342a1bc1") ?? 
                               await roleManager.FindByNameAsync("SuperAdmin");
                if (adminRole != null)
                {
                    var userRoles = await userManager.GetRolesAsync(existingAdmin);
                    if (!userRoles.Contains(adminRole.Name))
                    {
                        await userManager.AddToRoleAsync(existingAdmin, adminRole.Name);
                        Console.WriteLine($"✅ Admin user added to SuperAdmin role");
                    }
                }
                Console.WriteLine($"✅ Admin user already exists: {adminEmail}");
            }
        }
    }
}
