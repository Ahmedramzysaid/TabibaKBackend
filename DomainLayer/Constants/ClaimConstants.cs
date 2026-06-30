namespace DomainLayer.Constants;

public class ClaimConstants
{
    public const string Permission = "Permission";

    public static readonly IReadOnlyList<string> AllPermissions = new[]
    {
        ViewPatients, AddPatient, EditPatient, DeletePatient,
        ViewDoctors, AddDoctor, EditDoctor, DeleteDoctor,
        ViewAppointments, CreateAppointment, EditAppointment, CancelAppointment, RescheduleAppointment, CompleteAppointment,
        ViewMedicalRecords, CreateMedicalRecord, EditMedicalRecord, DeleteMedicalRecord,
        ViewPrescriptions, CreatePrescription, EditPrescription, DeletePrescription,
        ViewPayments, ProcessPayment,
        ViewDoctorAdviceVideos, CreateDoctorAdviceVideo, EditDoctorAdviceVideo, DeleteDoctorAdviceVideo
    };

    public const string ViewPatients = "view-patients";
    public const string AddPatient = "add-patient";
    public const string EditPatient = "edit-patient";
    public const string DeletePatient = "delete-patient";

    public const string ViewDoctors = "view-doctors";
    public const string AddDoctor = "add-doctor";
    public const string EditDoctor = "edit-doctor";
    public const string DeleteDoctor = "delete-doctor";

    public const string ViewAppointments = "view-appointments";
    public const string CreateAppointment = "create-appointment";
    public const string EditAppointment = "edit-appointment";
    public const string CancelAppointment = "cancel-appointment";
    public const string RescheduleAppointment = "reschedule-appointment";
    public const string CompleteAppointment = "complete-appointment";

    public const string ViewMedicalRecords = "view-medical-records";
    public const string CreateMedicalRecord = "create-medical-record";
    public const string EditMedicalRecord = "edit-medical-record";
    public const string DeleteMedicalRecord = "delete-medical-record";

    public const string ViewPrescriptions = "view-prescriptions";
    public const string CreatePrescription = "create-prescription";
    public const string EditPrescription = "edit-prescription";
    public const string DeletePrescription = "delete-prescription";

    public const string ViewPayments = "view-payments";
    public const string ProcessPayment = "process-payment";

    public const string ViewDoctorAdviceVideos = "view-doctor-advice-videos";
    public const string CreateDoctorAdviceVideo = "create-doctor-advice-video";
    public const string EditDoctorAdviceVideo = "edit-doctor-advice-video";
    public const string DeleteDoctorAdviceVideo = "delete-doctor-advice-video";
}
