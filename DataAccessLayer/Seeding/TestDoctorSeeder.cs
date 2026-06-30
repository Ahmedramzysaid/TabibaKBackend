using DataAccessLayer.Persistence;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Seeding;

public static class TestDoctorSeeder
{
    public const string Email = "testdoctor@tabibak.com";
    public const string Password = "TestDoctor123###";

    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ApplicationDbContext context)
    {
        var existingUser = await userManager.FindByEmailAsync(Email);
        if (existingUser != null)
        {
            Console.WriteLine($"✅ Test doctor already exists: {Email}");
            return;
        }

        Console.WriteLine($"🩺 Creating test doctor: {Email}...");

        var user = new ApplicationUser
        {
            FullName = "Dr. Test Doctor",
            Email = Email,
            UserName = Email,
            EmailConfirmed = true,
            DateOfBirth = new DateTime(1985, 6, 15),
            Gender = "Male",
            DateOfRegistration = DateTime.UtcNow,
            Latitude = 30.0444,
            Longitude = 31.2357,
            PhoneNumber = "01234567890"
        };

        var createResult = await userManager.CreateAsync(user, Password);
        if (!createResult.Succeeded)
        {
            Console.WriteLine($"❌ Failed to create test doctor: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
            return;
        }

        var doctorRole = await roleManager.FindByNameAsync("Doctor");
        if (doctorRole != null)
        {
            await userManager.AddToRoleAsync(user, "Doctor");
            Console.WriteLine("   ✅ Assigned 'Doctor' role");
        }

        var doctor = new Doctor
        {
            Id = user.Id,
            Specialization = "General Medicine",
            IdNo = "TEST-001",
            Price = 250m,
            IsAvailable = true,
            Rating = 4.5m,
            RatingCount = 12
        };
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();
        Console.WriteLine("   ✅ Created Doctor record");

        var workingDays = new[]
        {
            (DayOfWeek.Sunday,    TimeSpan.FromHours(9),  TimeSpan.FromHours(17)),
            (DayOfWeek.Monday,    TimeSpan.FromHours(9),  TimeSpan.FromHours(17)),
            (DayOfWeek.Tuesday,   TimeSpan.FromHours(9),  TimeSpan.FromHours(17)),
            (DayOfWeek.Wednesday, TimeSpan.FromHours(9),  TimeSpan.FromHours(17)),
            (DayOfWeek.Thursday,  TimeSpan.FromHours(9),  TimeSpan.FromHours(17)),
            (DayOfWeek.Friday,    TimeSpan.FromHours(10), TimeSpan.FromHours(14)),
        };

        foreach (var (day, start, end) in workingDays)
        {
            context.DoctorSchedules.Add(new DoctorSchedule
            {
                DoctorId = user.Id,
                DayOfWeek = day,
                StartTime = start,
                EndTime = end,
                SlotDurationMinutes = 30,
                IsActive = day != DayOfWeek.Friday // Friday off by default
            });
        }

        context.DoctorSchedules.Add(new DoctorSchedule
        {
            DoctorId = user.Id,
            DayOfWeek = DayOfWeek.Saturday,
            StartTime = TimeSpan.FromHours(10),
            EndTime = TimeSpan.FromHours(14),
            SlotDurationMinutes = 30,
            IsActive = false
        });
        Console.WriteLine("   📅 Seeded weekly schedule");

        var random = new Random(123);
        var today = DateTime.UtcNow.Date;
        var earningsCount = 0;

        for (int daysAgo = 90; daysAgo >= 0; daysAgo--)
        {
            var date = today.AddDays(-daysAgo);
            if (date.DayOfWeek == DayOfWeek.Saturday) continue;

            int maxAppts = date.DayOfWeek == DayOfWeek.Friday ? 3 : 8;
            int apptCount = random.Next(1, maxAppts + 1);
            var dailyEarnings = apptCount * 250m;

            context.DoctorEarnings.Add(new DoctorEarning
            {
                DoctorId = user.Id,
                Date = date,
                DailyEarnings = dailyEarnings,
                AppointmentCount = apptCount
            });
            earningsCount++;
        }

        await context.SaveChangesAsync();
        Console.WriteLine($"   💰 Seeded {earningsCount} days of earnings data");
        Console.WriteLine($"✅ Test doctor created successfully!");
        Console.WriteLine($"   📧 Email: {Email}");
        Console.WriteLine($"   🔑 Password: {Password}");
    }
}
