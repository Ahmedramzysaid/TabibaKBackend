namespace DomainLayer.Constants;

public class AuthorizationPolicies
{
    public const string CanViewPatients = "CanViewPatients";
    public const string CanAddPatient = "CanAddPatient";
    public const string CanEditPatient = "CanEditPatient";
    public const string CanDeletePatient = "CanDeletePatient";

    public const string CanViewDoctors = "CanViewDoctors";
    public const string CanAddDoctor = "CanAddDoctor";
    public const string CanEditDoctor = "CanEditDoctor";
    public const string CanDeleteDoctor = "CanDeleteDoctor";

    public const string CanViewAppointments = "CanViewAppointments";
    public const string CanCreateAppointment = "CanCreateAppointment";
    public const string CanEditAppointment = "CanEditAppointment";
    public const string CanCancelAppointment = "CanCancelAppointment";
    public const string CanRescheduleAppointment = "CanRescheduleAppointment";
    public const string CanCompleteAppointment = "CanCompleteAppointment";

    public const string CanViewMedicalRecords = "CanViewMedicalRecords";
    public const string CanCreateMedicalRecord = "CanCreateMedicalRecord";
    public const string CanEditMedicalRecord = "CanEditMedicalRecord";
    public const string CanDeleteMedicalRecord = "CanDeleteMedicalRecord";

    public const string CanViewPrescriptions = "CanViewPrescriptions";
    public const string CanCreatePrescription = "CanCreatePrescription";
    public const string CanEditPrescription = "CanEditPrescription";
    public const string CanEditDigitalPrescription = "CanEditDigitalPrescription";
    public const string CanCreateDigitalPrescription = "CanCreateDigitalPrescription";
    public const string CanDeletePrescription = "CanDeletePrescription";
    public const string CanDeleteDigitalPrescription = "CanDeleteDigitalPrescription";

    public const string CanViewPayments = "CanViewPayments";
    public const string CanProcessPayment = "CanProcessPayment";

    public const string CanViewDoctorAdviceVideos = "CanViewDoctorAdviceVideos";
    public const string CanCreateDoctorAdviceVideo = "CanCreateDoctorAdviceVideo";
    public const string CanEditDoctorAdviceVideo = "CanEditDoctorAdviceVideo";
    public const string CanDeleteDoctorAdviceVideo = "CanDeleteDoctorAdviceVideo";
}
