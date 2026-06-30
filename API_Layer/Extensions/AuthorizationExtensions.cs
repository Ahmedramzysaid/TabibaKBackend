using System.Security.Claims;
using DomainLayer.Constants;
using Microsoft.AspNetCore.Authorization;
using ClinicAPI.Middlewares;

namespace ClinicAPI.Extensions;

public static class AuthorizationExtensions
{
    private static bool HasPermission(ClaimsPrincipal user, string permissionValue)
    {
        if (user.HasClaim(ClaimConstants.Permission, permissionValue))
            return true;
        var combined = user.FindFirst(ClaimConstants.Permission)?.Value;
        if (string.IsNullOrEmpty(combined))
            return false;
        return combined.Split(',').Any(p => string.Equals(p.Trim(), permissionValue, StringComparison.OrdinalIgnoreCase));
    }

    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddSingleton<IAuthorizationHandler, SuperAdminAuthorizationHandler>();

        services.AddAuthorization(options =>
        {
            static Action<Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder, string> RequireSuperAdminOrClaim() =>
                (builder, claimValue) => builder.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, claimValue));

            options.AddPolicy(AuthorizationPolicies.CanAddPatient,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.AddPatient));
            options.AddPolicy(AuthorizationPolicies.CanViewPatients,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.ViewPatients));
            options.AddPolicy(AuthorizationPolicies.CanEditPatient,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.EditPatient));
            options.AddPolicy(AuthorizationPolicies.CanDeletePatient,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.DeletePatient));

            options.AddPolicy(AuthorizationPolicies.CanViewDoctors,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.ViewDoctors));
            options.AddPolicy(AuthorizationPolicies.CanAddDoctor,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.AddDoctor));
            options.AddPolicy(AuthorizationPolicies.CanEditDoctor,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.EditDoctor));
            options.AddPolicy(AuthorizationPolicies.CanDeleteDoctor,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.DeleteDoctor));

            options.AddPolicy(AuthorizationPolicies.CanViewAppointments,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.ViewAppointments));
            options.AddPolicy(AuthorizationPolicies.CanCreateAppointment,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.CreateAppointment));
            options.AddPolicy(AuthorizationPolicies.CanEditAppointment,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.EditAppointment));
            options.AddPolicy(AuthorizationPolicies.CanCancelAppointment,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.CancelAppointment));
            options.AddPolicy(AuthorizationPolicies.CanRescheduleAppointment,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.RescheduleAppointment));
            options.AddPolicy(AuthorizationPolicies.CanCompleteAppointment,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.CompleteAppointment));

            options.AddPolicy(AuthorizationPolicies.CanViewMedicalRecords,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.ViewMedicalRecords) ||
                    ctx.User.IsInRole(Roles.Patient) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanCreateMedicalRecord,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.CreateMedicalRecord) ||
                    ctx.User.IsInRole(Roles.Patient)));
            options.AddPolicy(AuthorizationPolicies.CanEditMedicalRecord,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.EditMedicalRecord) ||
                    ctx.User.IsInRole(Roles.Patient)));
            options.AddPolicy(AuthorizationPolicies.CanDeleteMedicalRecord,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.DeleteMedicalRecord) ||
                    ctx.User.IsInRole(Roles.Patient)));

            options.AddPolicy(AuthorizationPolicies.CanViewPrescriptions,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.ViewPrescriptions) ||
                    ctx.User.IsInRole(Roles.Patient) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanCreatePrescription,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.CreatePrescription) ||
                    ctx.User.IsInRole(Roles.Patient) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanEditPrescription,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.EditPrescription) ||
                    ctx.User.IsInRole(Roles.Patient) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanEditDigitalPrescription,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.EditPrescription) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanCreateDigitalPrescription,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.CreatePrescription) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanDeletePrescription,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.DeletePrescription) ||
                    ctx.User.IsInRole(Roles.Patient)));
            options.AddPolicy(AuthorizationPolicies.CanDeleteDigitalPrescription,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.DeletePrescription) ||
                    ctx.User.IsInRole(Roles.Patient)));

            options.AddPolicy(AuthorizationPolicies.CanViewPayments,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.ViewPayments));
            options.AddPolicy(AuthorizationPolicies.CanProcessPayment,
                policy => RequireSuperAdminOrClaim()(policy, ClaimConstants.ProcessPayment));

            options.AddPolicy(AuthorizationPolicies.CanViewDoctorAdviceVideos,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.ViewDoctorAdviceVideos) ||
                    ctx.User.IsInRole(Roles.Doctor) ||
                    ctx.User.IsInRole(Roles.Patient)));

            options.AddPolicy(AuthorizationPolicies.CanCreateDoctorAdviceVideo,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.CreateDoctorAdviceVideo) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanEditDoctorAdviceVideo,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.EditDoctorAdviceVideo) ||
                    ctx.User.IsInRole(Roles.Doctor)));
            options.AddPolicy(AuthorizationPolicies.CanDeleteDoctorAdviceVideo,
                policy => policy.RequireAssertion(ctx =>
                    ctx.User.IsInRole(Roles.SuperAdmin) ||
                    HasPermission(ctx.User, ClaimConstants.DeleteDoctorAdviceVideo) ||
                    ctx.User.IsInRole(Roles.Doctor)));
        });

        return services;
    }
}
