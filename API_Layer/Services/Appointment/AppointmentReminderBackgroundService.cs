using BusinessLayer;
using DataAccessLayer.Persistence;
using DomainLayer.Enums;
using DomainLayer.Helpers;
using DomainLayer.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Services;

public class AppointmentReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AppointmentReminderBackgroundService> _logger;
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    public AppointmentReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<AppointmentReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Appointment reminder background service started. Checking every {IntervalMinutes} minute(s).",
            (int)Interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessRemindersAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in appointment reminder loop.");
            }

            await Task.Delay(Interval, stoppingToken);
        }
    }

    private async Task ProcessRemindersAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var now = DateTimeOffset.UtcNow;
        var utcNow = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(utcNow);
        var nowTime = utcNow.TimeOfDay;

        var due = await db.Appointments
            .AsNoTracking()
            .Where(a => a.ReminderAt.HasValue
                && a.ReminderAt.Value <= now
                && !a.ReminderSent
                && a.AppointmentStatus != (short)AppointmentStatus.Canceled
                && (a.AppointmentDate > today
                    || (a.AppointmentDate == today && a.AppointmentTime > nowTime)))
            .Select(a => new
            {
                a.AppointmentID,
                a.PatientID,
                a.DoctorID,
                a.AppointmentDate,
                a.AppointmentTime,
                a.ReminderAt
            })
            .ToListAsync(ct);

        if (due.Count == 0)
            return;

        var sentCount = 0;
        foreach (var item in due)
        {
            if (ct.IsCancellationRequested)
                break;
            try
            {
                var patient = await db.Patients
                    .Include(p => p.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == item.PatientID, ct);
                var doctor = await db.Doctors
                    .Include(d => d.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.Id == item.DoctorID, ct);

                var patientEmail = patient?.User?.Email?.Trim();
                if (string.IsNullOrEmpty(patientEmail))
                {
                    await MarkReminderSentAsync(db, item.AppointmentID, ct);
                    continue;
                }

                var patientName = patient?.User?.FullName ?? "Patient";
                var doctorName = doctor?.User?.FullName ?? "Doctor";
                var appointmentUtc = AppointmentScheduling.ToUtcDateTime(item.AppointmentDate, item.AppointmentTime);
                var minutesUntil = (int)Math.Max(0, (appointmentUtc - utcNow).TotalMinutes);
                if (minutesUntil <= 0) minutesUntil = 1;

                var subject = "Appointment reminder – Tabibak";
                var body = EmailTemplates.AppointmentReminder(patientName, doctorName, appointmentUtc, minutesUntil);
                var sent = await emailService.SendEmailAsync(patientEmail, subject, body, isHtml: true);
                if (sent)
                    sentCount++;
                await MarkReminderSentAsync(db, item.AppointmentID, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send reminder for appointment {AppointmentId}.", item.AppointmentID);
            }
        }

        if (sentCount > 0)
            _logger.LogInformation("Sent {SentCount} appointment reminder(s).", sentCount);
    }

    private static async Task MarkReminderSentAsync(ApplicationDbContext db, Guid appointmentId, CancellationToken ct)
    {
        var appointment = await db.Appointments.FirstOrDefaultAsync(a => a.AppointmentID == appointmentId, ct);
        if (appointment != null)
        {
            appointment.ReminderSent = true;
            await db.SaveChangesAsync(ct);
        }
    }
}
