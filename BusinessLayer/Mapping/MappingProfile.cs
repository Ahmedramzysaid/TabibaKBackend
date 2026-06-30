using AutoMapper;
using DomainLayer.DTOs;
using DomainLayer.Models;

namespace BusinessLayer.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Patient, PatientDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dest => dest.DateOfRegistration, opt => opt.MapFrom(src => src.User.DateOfRegistration))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User.DateOfBirth))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User.Gender))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.User.Latitude))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.User.Longitude))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User.Email))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User.ProfileImageUrl));
            
            CreateMap<PatientDto, Patient>()
                .ForMember(dest => dest.User, opt => opt.Ignore());
            
            CreateMap<Doctor, DoctorDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.DateOfRegistration, opt => opt.MapFrom(src => src.User != null ? src.User.DateOfRegistration : default))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.User != null ? src.User.DateOfBirth : default(DateTime?)))
                .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => src.User != null ? src.User.Gender : string.Empty))
                .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.User != null ? src.User.Latitude : default(double?)))
                .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.User != null ? src.User.Longitude : default(double?)))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : null))
                .ForMember(dest => dest.ProfileImageUrl, opt => opt.MapFrom(src => src.User != null ? src.User.ProfileImageUrl : null));
            
            CreateMap<DoctorDto, Doctor>()
                .ForMember(dest => dest.User, opt => opt.Ignore())
                .ForMember(dest => dest.Rating, opt => opt.Ignore())
                .ForMember(dest => dest.RatingCount, opt => opt.Ignore());
            
            CreateMap<Appointment, AppointmentDto>()
                .ForMember(dest => dest.MedicalRecordDto, opt => opt.Ignore());
            CreateMap<CreateAppointmentDto, AppointmentDto>()
                .ForMember(dest => dest.AppointmentStatus, opt => opt.MapFrom(src => (short)(src.AppointmentStatus == 0 ? 1 : src.AppointmentStatus)))
                .ForMember(dest => dest.ReminderMinutesBefore, opt => opt.MapFrom(src => src.ReminderMinutesBefore));
            CreateMap<AppointmentDto, Appointment>()
                .ForMember(dest => dest.MedicalRecord, opt => opt.Ignore())
                .ForMember(dest => dest.AppointmentID, opt => opt.Ignore())
                .ForMember(dest => dest.ReminderAt, opt => opt.Ignore())
                .ForMember(dest => dest.ReminderSent, opt => opt.Ignore());
            CreateMap<CompletePrescriptionDto, Prescription>().ReverseMap();
            CreateMap<CompletePaymentDto, Payment>().ReverseMap();
            CreateMap<PaymentDto, Payment>().ReverseMap();
            
            CreateMap<MedicalRecord, MedicalRecordDto>().ReverseMap();
            
            CreateMap<Prescription, PrescriptionDto>().ReverseMap();
            CreateMap<CreatePrescriptionDto, Prescription>()
                .ForMember(dest => dest.PrescriptionId, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecord, opt => opt.Ignore());
            CreateMap<CreateOrUpdatePrescriptionDto, Prescription>()
                .ForMember(dest => dest.PrescriptionId, opt => opt.MapFrom(src => src.PrescriptionId == Guid.Empty ? Guid.NewGuid() : src.PrescriptionId))
                .ForMember(dest => dest.MedicalRecord, opt => opt.Ignore());

            CreateMap<DigitalPrescriptionItem, DigitalPrescriptionItemDto>();
            CreateMap<CreateDigitalPrescriptionItemDto, DigitalPrescriptionItem>()
                .ForMember(dest => dest.DigitalPrescriptionItemId, opt => opt.Ignore())
                .ForMember(dest => dest.DigitalPrescriptionId, opt => opt.Ignore());
            CreateMap<DigitalPrescription, DigitalPrescriptionDto>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.PatientId, opt => opt.MapFrom(src => src.MedicalRecord != null ? src.MedicalRecord.PatientId : string.Empty));
            CreateMap<CreateDigitalPrescriptionDto, DigitalPrescription>()
                .ForMember(dest => dest.DigitalPrescriptionId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.MedicalRecord, opt => opt.Ignore())
                .ForMember(dest => dest.Items, opt => opt.Ignore());

            CreateMap<DoctorAdviceVideo, DoctorAdviceVideoDto>()
                .ForMember(dest => dest.DoctorName, opt => opt.MapFrom(src => src.Doctor != null && src.Doctor.User != null ? src.Doctor.User.FullName : null));
            CreateMap<CreateDoctorAdviceVideoDto, DoctorAdviceVideo>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.DoctorId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.Doctor, opt => opt.Ignore());
            
        }
    }
}
