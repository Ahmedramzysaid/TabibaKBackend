using DataAccessLayer.Persistence;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Seeding;

public static class DoctorDashboardSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.DoctorSchedules.AnyAsync() || await context.DoctorEarnings.AnyAsync())
        {
            Console.WriteLine("✅ Doctor dashboard data already seeded — skipping.");
            return;
        }

        var doctors = await context.Doctors.AsNoTracking().Take(10).ToListAsync();
        if (!doctors.Any())
        {
            Console.WriteLine("⚠️  No doctors found — skipping dashboard seeding.");
            return;
        }

        Console.WriteLine($"🩺 Seeding dashboard data for {doctors.Count} doctors...");

        var schedules = new List<DoctorSchedule>();
        foreach (var doctor in doctors)
        {
            var workingDays = new[]
            {
                DayOfWeek.Sunday,
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday
            };

            foreach (var day in workingDays)
            {
                schedules.Add(new DoctorSchedule
                {
                    DoctorId = doctor.Id,
                    DayOfWeek = day,
                    StartTime = TimeSpan.FromHours(9),   // 09:00 AM
                    EndTime = TimeSpan.FromHours(17),     // 05:00 PM
                    SlotDurationMinutes = 30,
                    IsActive = true
                });
            }

            schedules.Add(new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Friday,
                StartTime = TimeSpan.FromHours(10),  // 10:00 AM
                EndTime = TimeSpan.FromHours(14),    // 02:00 PM
                SlotDurationMinutes = 30,
                IsActive = false // Day off by default
            });

            schedules.Add(new DoctorSchedule
            {
                DoctorId = doctor.Id,
                DayOfWeek = DayOfWeek.Saturday,
                StartTime = TimeSpan.FromHours(10),
                EndTime = TimeSpan.FromHours(14),
                SlotDurationMinutes = 30,
                IsActive = false
            });
        }

        await context.DoctorSchedules.AddRangeAsync(schedules);
        Console.WriteLine($"   📅 Seeded {schedules.Count} schedule entries.");

        var earnings = new List<DoctorEarning>();
        var random = new Random(42); // Fixed seed for reproducibility
        var today = DateTime.UtcNow.Date;

        foreach (var doctor in doctors)
        {
            var price = doctor.Price ?? 200m; // Default 200 EGP if not set

            for (int daysAgo = 90; daysAgo >= 0; daysAgo--)
            {
                var date = today.AddDays(-daysAgo);
                var dayOfWeek = date.DayOfWeek;

                if (dayOfWeek == DayOfWeek.Saturday) continue;

                int maxAppointments = dayOfWeek == DayOfWeek.Friday ? 3 : 8;
                int appointmentCount = random.Next(0, maxAppointments + 1);

                if (appointmentCount == 0) continue; // Some days have 0

                var dailyEarnings = appointmentCount * price;

                earnings.Add(new DoctorEarning
                {
                    DoctorId = doctor.Id,
                    Date = date,
                    DailyEarnings = dailyEarnings,
                    AppointmentCount = appointmentCount
                });
            }
        }

        await context.DoctorEarnings.AddRangeAsync(earnings);
        Console.WriteLine($"   💰 Seeded {earnings.Count} earnings records (last 90 days).");

        await context.SaveChangesAsync();
        Console.WriteLine("✅ Doctor dashboard seeding completed!");
    }
}
