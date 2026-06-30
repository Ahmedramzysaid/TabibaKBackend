using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Seeding;

public static class UpdateSuperAdminName
{
    public static async Task UpdateAsync(UserManager<ApplicationUser> userManager)
    {
        var superAdminUsers = await userManager.GetUsersInRoleAsync("SuperAdmin");
        var drSaraUser = superAdminUsers.FirstOrDefault(u => u.FullName == "Dr.Sara" || u.FullName.Contains("Dr.Sara") || u.FullName.Contains("Sara"));

        if (drSaraUser != null)
        {
            drSaraUser.FullName = "Dr.Ahmed Ramzy";
            var updateResult = await userManager.UpdateAsync(drSaraUser);
            
            if (updateResult.Succeeded)
            {
                Console.WriteLine($"✅ Successfully updated user name from 'Dr.Sara' to 'Dr.Ahmed Ramzy' (ID: {drSaraUser.Id})");
            }
            else
            {
                Console.WriteLine($"❌ Failed to update user name: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            Console.WriteLine("ℹ️ User 'Dr.Sara' not found in SuperAdmin role. Searching all users...");
            
            var allUsers = await userManager.Users.ToListAsync();
            var saraUser = allUsers.FirstOrDefault(u => u.FullName == "Dr.Sara" || u.FullName.Contains("Dr.Sara") || u.FullName.Contains("Sara"));
            
            if (saraUser != null)
            {
                saraUser.FullName = "Dr.Ahmed Ramzy";
                var updateResult = await userManager.UpdateAsync(saraUser);
                
                if (updateResult.Succeeded)
                {
                    Console.WriteLine($"✅ Successfully updated user name from 'Dr.Sara' to 'Dr.Ahmed Ramzy' (ID: {saraUser.Id})");
                }
                else
                {
                    Console.WriteLine($"❌ Failed to update user name: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                }
            }
            else
            {
                Console.WriteLine("⚠️ User with name containing 'Dr.Sara' or 'Sara' not found in database.");
            }
        }
    }
}
