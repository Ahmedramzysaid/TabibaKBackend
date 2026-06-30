using System.Security.Claims;
using DataAccessLayer.Persistence;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Seeding;

public class PermissionSeeder
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _dbContext;

    public PermissionSeeder(ApplicationDbContext dbContext, RoleManager<IdentityRole> roleManager)
    {
        _dbContext = dbContext;
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        var Roleclaims = new Dictionary<string, List<string>>();

        Roleclaims.Add(Roles.SuperAdmin, ClaimConstants.AllPermissions.ToList());

        Roleclaims.Add(Roles.ClinicManager, new List<string>
        {
            ClaimConstants.AddPatient,
            ClaimConstants.EditPatient,
            ClaimConstants.DeletePatient,
            ClaimConstants.ViewPatients,

            ClaimConstants.ViewDoctors,
            ClaimConstants.AddDoctor,
            ClaimConstants.EditDoctor,
            ClaimConstants.DeleteDoctor,

            ClaimConstants.ViewAppointments,
            ClaimConstants.CreateAppointment,
            ClaimConstants.EditAppointment,
            ClaimConstants.CancelAppointment,
            ClaimConstants.CompleteAppointment,

            ClaimConstants.ViewMedicalRecords,
            ClaimConstants.CreateMedicalRecord,
            ClaimConstants.EditMedicalRecord,
            ClaimConstants.DeleteMedicalRecord,

            ClaimConstants.ViewPrescriptions,
            ClaimConstants.CreatePrescription,
            ClaimConstants.EditPrescription,
            ClaimConstants.DeletePrescription,

            ClaimConstants.ViewPayments,
            ClaimConstants.ProcessPayment,

            ClaimConstants.ViewDoctorAdviceVideos,
            ClaimConstants.CreateDoctorAdviceVideo,
            ClaimConstants.EditDoctorAdviceVideo,
            ClaimConstants.DeleteDoctorAdviceVideo
        });

        Roleclaims.Add(Roles.Receptionist, new List<string>
        {
            ClaimConstants.AddPatient,
            ClaimConstants.EditPatient,
            ClaimConstants.ViewPatients,

            ClaimConstants.ViewDoctors,

            ClaimConstants.ViewAppointments,
            ClaimConstants.CreateAppointment,
            ClaimConstants.EditAppointment,
            ClaimConstants.CancelAppointment,
            ClaimConstants.CompleteAppointment,

            ClaimConstants.ViewMedicalRecords,

            ClaimConstants.ViewPrescriptions,

            ClaimConstants.ViewPayments,
            ClaimConstants.ProcessPayment,

            ClaimConstants.ViewDoctorAdviceVideos
        });

        Roleclaims.Add(Roles.MedicalAdmin, new List<string>
        {
            ClaimConstants.AddPatient,
            ClaimConstants.EditPatient,
            ClaimConstants.ViewPatients,

            ClaimConstants.ViewDoctors,

            ClaimConstants.ViewAppointments,

            ClaimConstants.ViewMedicalRecords,
            ClaimConstants.CreateMedicalRecord,
            ClaimConstants.EditMedicalRecord,

            ClaimConstants.ViewPrescriptions,
            ClaimConstants.CreatePrescription,
            ClaimConstants.EditPrescription,

            ClaimConstants.ViewDoctorAdviceVideos
        });

        Roleclaims.Add(Roles.Doctor, new List<string>
        {
            ClaimConstants.ViewPatients,

            ClaimConstants.ViewDoctors,
            ClaimConstants.EditDoctor,

            ClaimConstants.ViewAppointments,
            ClaimConstants.CreateAppointment,
            ClaimConstants.EditAppointment,
            ClaimConstants.CancelAppointment,
            ClaimConstants.CompleteAppointment,

            ClaimConstants.ViewMedicalRecords,
            ClaimConstants.CreateMedicalRecord,
            ClaimConstants.EditMedicalRecord,

            ClaimConstants.ViewPrescriptions,
            ClaimConstants.CreatePrescription,
            ClaimConstants.EditPrescription,

            ClaimConstants.ViewDoctorAdviceVideos,
            ClaimConstants.CreateDoctorAdviceVideo,
            ClaimConstants.EditDoctorAdviceVideo,
            ClaimConstants.DeleteDoctorAdviceVideo
        });

        Roleclaims.Add(Roles.Patient, new List<string>
        {
            ClaimConstants.EditPatient,

            ClaimConstants.ViewAppointments,
            ClaimConstants.CreateAppointment,
            ClaimConstants.RescheduleAppointment,
            ClaimConstants.CancelAppointment,

            ClaimConstants.ViewMedicalRecords,

            ClaimConstants.ViewPrescriptions,

            ClaimConstants.ViewPayments,

            ClaimConstants.ViewDoctorAdviceVideos
        });

        foreach (var roleName in Roleclaims.Keys)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            if (role is not null)
            {
                var existingClaims = await _roleManager.GetClaimsAsync(role);
                var existingClaimValues = existingClaims
                    .Where(c => c.Type == ClaimConstants.Permission)
                    .Select(c => c.Value)
                    .ToHashSet();
                
                foreach (var claim in Roleclaims[roleName])
                {
                    if (!existingClaimValues.Contains(claim))
                    {
                        await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, claim));
                    }
                }
            }
        }

        await EnsureSuperAdminHasAllClaimsAsync();
    }

    private async Task EnsureSuperAdminHasAllClaimsAsync()
    {
        var role = await _roleManager.FindByNameAsync(Roles.SuperAdmin);
        if (role is null) return;

        var existingClaims = await _roleManager.GetClaimsAsync(role);
        var existingClaimValues = existingClaims
            .Where(c => c.Type == ClaimConstants.Permission)
            .Select(c => c.Value)
            .ToHashSet();

        foreach (var claim in ClaimConstants.AllPermissions)
        {
            if (!existingClaimValues.Contains(claim))
                await _roleManager.AddClaimAsync(role, new Claim(ClaimConstants.Permission, claim));
        }
    }
}
