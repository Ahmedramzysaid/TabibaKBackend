using System.Globalization;
using DomainLayer.Constants;
using DomainLayer.Enums;
using DomainLayer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DataAccessLayer.Persistence;
using NetTopologySuite.Geometries;

namespace ClinicAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeedingController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public SeedingController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpPost("doctors")]
        public async Task<IActionResult> SeedDoctors()
        {
            try
            {
                var adminRole = await _roleManager.FindByNameAsync(Roles.SuperAdmin);
                if (adminRole == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(Roles.SuperAdmin));
                }

                var doctorRole = await _roleManager.FindByNameAsync(Roles.Doctor);
                if (doctorRole == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(Roles.Doctor));
                }

                var cities = GetEgyptianCities();
                var doctorsData = ReadDoctorsFromCsv("wwwroot/VervicationDoctor/data_clean.csv");

                if (!doctorsData.Any())
                {
                    return BadRequest("No doctors found in the CSV file.");
                }

                var groupedBySpecialty = doctorsData
                    .GroupBy(d => d.Specialization)
                    .Where(g => g.Count() >= 10) // Only take specialties that have at least 10 doctors
                    .ToList();

                if (groupedBySpecialty.Count < 1)
                {
                    return BadRequest("Not enough unique specializations in the CSV file.");
                }

                var random = new Random();
                int totalCreated = 0;
                var password = "Password123!";
                var createdDoctors = new List<object>();

                var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

                foreach (var city in cities)
                {
                    var citySpecialties = groupedBySpecialty.OrderBy(x => random.Next()).Take(1).ToList();

                    foreach (var specialtyGroup in citySpecialties)
                    {
                        var selectedDoctors = specialtyGroup.OrderBy(x => random.Next()).Take(10).ToList();

                        foreach (var docData in selectedDoctors)
                        {
                            var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                            var email = $"doctor_{uniqueId}@tabibak.com";
                            var phone = "01" + random.Next(100000000, 999999999).ToString();

                            var latOffset = (random.NextDouble() * 0.18) - 0.09; 
                            var lonOffset = (random.NextDouble() * 0.18) - 0.09;
                            var docLat = city.Latitude + latOffset;
                            var docLon = city.Longitude + lonOffset;

                            var applicationUser = new ApplicationUser
                            {
                                FullName = docData.Name,
                                Email = email,
                                UserName = phone,
                                PhoneNumber = phone,
                                EmailConfirmed = true,
                                DateOfRegistration = DateTime.UtcNow,
                                DateOfBirth = new DateTime(random.Next(1960, 2000), random.Next(1, 13), random.Next(1, 29)),
                                Gender = random.Next(2) == 0 ? "Male" : "Female",
                                Latitude = docLat,
                                Longitude = docLon,
                                Location = geometryFactory.CreatePoint(new Coordinate(docLon, docLat)),
                                ProfileImageUrl = $"https://i.pravatar.cc/150?u={uniqueId}"
                            };

                            var createResult = await _userManager.CreateAsync(applicationUser, password);
                            if (createResult.Succeeded)
                            {
                                await _userManager.AddToRoleAsync(applicationUser, Roles.Doctor);

                                var doctor = new Doctor
                                {
                                    Id = applicationUser.Id,
                                    Specialization = docData.Specialization,
                                    IdNo = random.Next(10000, 99999).ToString(),
                                    Price = random.Next(100, 1000),
                                    IsAvailable = true,
                                    Rating = (decimal)(random.NextDouble() * 4.0 + 1.0),
                                    RatingCount = random.Next(5, 501)
                                };

                                _context.Doctors.Add(doctor);
                                createdDoctors.Add(new { id = applicationUser.Id, phone, email, name = docData.Name, specialization = docData.Specialization, city = city.Name });
                                totalCreated++;
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new 
                { 
                    message = $"Successfully seeded {totalCreated} doctors.",
                    password,
                    sampleDoctors = createdDoctors.Take(10),
                    allDoctors = createdDoctors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        [HttpPost("patients")]
        public async Task<IActionResult> SeedPatients()
        {
            try
            {
                var patientRole = await _roleManager.FindByNameAsync(Roles.Patient);
                if (patientRole == null)
                {
                    await _roleManager.CreateAsync(new IdentityRole(Roles.Patient));
                }

                var random = new Random();
                var password = "Password123!";
                var geometryFactory = NetTopologySuite.NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                var cities = GetEgyptianCities();

                var firstNames = new[] { "Ahmed", "Mohamed", "Ali", "Omar", "Youssef", "Hassan", "Khaled", "Ibrahim", "Mahmoud", "Mostafa",
                    "Fatma", "Nour", "Sara", "Mona", "Hana", "Amira", "Dina", "Rania", "Laila", "Yasmin" };
                var lastNames = new[] { "Hassan", "Ali", "Mohamed", "Ibrahim", "Mostafa", "Youssef", "Khaled", "Samir", "Nabil", "Adel",
                    "Salem", "Farid", "Gamal", "Ramzy", "Tamer", "Sherif", "Waleed", "Hossam", "Emad", "Ashraf" };
                var bloodTypes = new[] { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

                int totalCreated = 0;
                var createdPatients = new List<object>();

                for (int i = 0; i < 100; i++)
                {
                    var uniqueId = Guid.NewGuid().ToString("N").Substring(0, 8);
                    var firstName = firstNames[random.Next(firstNames.Length)];
                    var lastName = lastNames[random.Next(lastNames.Length)];
                    var fullName = $"{firstName} {lastName}";
                    var email = $"patient_{uniqueId}@tabibak.com";
                    var phone = "01" + random.Next(100000000, 999999999).ToString();
                    var city = cities[random.Next(cities.Count)];

                    var latOffset = (random.NextDouble() * 0.18) - 0.09;
                    var lonOffset = (random.NextDouble() * 0.18) - 0.09;
                    var lat = city.Latitude + latOffset;
                    var lon = city.Longitude + lonOffset;

                    var applicationUser = new ApplicationUser
                    {
                        FullName = fullName,
                        Email = email,
                        UserName = phone,
                        PhoneNumber = phone,
                        EmailConfirmed = true,
                        DateOfRegistration = DateTime.UtcNow,
                        Gender = random.Next(2) == 0 ? "Male" : "Female",
                        Latitude = lat,
                        Longitude = lon,
                        Location = geometryFactory.CreatePoint(new Coordinate(lon, lat)),
                        ProfileImageUrl = $"https://i.pravatar.cc/150?u={uniqueId}",
                        DateOfBirth = new DateTime(random.Next(1960, 2005), random.Next(1, 13), random.Next(1, 29))
                    };

                    var createResult = await _userManager.CreateAsync(applicationUser, password);
                    if (createResult.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(applicationUser, Roles.Patient);

                        var patient = new Patient
                        {
                            Id = applicationUser.Id
                        };

                        _context.Patients.Add(patient);
                        createdPatients.Add(new { id = applicationUser.Id, phone, email, name = fullName, city = city.Name });
                        totalCreated++;
                    }

                    if ((i + 1) % 20 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new 
                { 
                    message = $"Successfully seeded {totalCreated} patients.",
                    password,
                    samplePatients = createdPatients.Take(10),
                    allPatients = createdPatients
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        [HttpPost("appointments")]
        public async Task<IActionResult> SeedAppointments()
        {
            try
            {
                var doctors = await _context.Doctors.Select(d => d.Id).ToListAsync();
                var patients = await _context.Patients.Select(p => p.Id).ToListAsync();

                if (!doctors.Any())
                    return BadRequest("No doctors found. Please seed doctors first.");
                if (!patients.Any())
                    return BadRequest("No patients found. Please seed patients first.");

                var random = new Random();
                int totalCreated = 0;
                var createdAppointments = new List<object>();

                foreach (var doctorId in doctors)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var patientId = patients[random.Next(patients.Count)];
                        bool isCompleted = i < 5; // First 5 = Completed, last 5 = Pending

                        var appointment = new Appointment
                        {
                            AppointmentID = Guid.NewGuid(),
                            DoctorID = doctorId,
                            PatientID = patientId,
                            AppointmentStatus = isCompleted
                                ? (short)AppointmentStatus.Completed
                                : (short)AppointmentStatus.Pending,
                            AppointmentDate = isCompleted
                                ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-random.Next(1, 90)))
                                : DateOnly.FromDateTime(DateTime.UtcNow.AddDays(random.Next(1, 30))),
                            AppointmentTime = TimeSpan.FromHours(random.Next(9, 17)),
                            AdditionalNotes = isCompleted
                                ? "Seeded completed appointment for testing."
                                : "Seeded pending appointment for testing."
                        };

                        _context.Appointments.Add(appointment);
                        createdAppointments.Add(new 
                        { 
                            appointmentId = appointment.AppointmentID, 
                            doctorId, 
                            patientId, 
                            status = isCompleted ? "Completed" : "Pending",
                            appointmentDate = appointment.AppointmentDate,
                            appointmentTime = appointment.AppointmentTime 
                        });
                        totalCreated++;
                    }

                    if (totalCreated % 500 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }

                await _context.SaveChangesAsync();
                return Ok(new 
                { 
                    message = $"Successfully seeded {totalCreated} appointments for {doctors.Count} doctors (10 each: 5 Completed + 5 Pending).",
                    sampleAppointments = createdAppointments.Take(20),
                    allAppointments = createdAppointments
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message} {ex.InnerException?.Message}");
            }
        }

        private List<DoctorCsvRecord> ReadDoctorsFromCsv(string filePath)
        {
            var absolutePath = Path.Combine(Directory.GetCurrentDirectory(), filePath);
            if (!System.IO.File.Exists(absolutePath))
                return new List<DoctorCsvRecord>();

            var doctors = new List<DoctorCsvRecord>();
            var lines = System.IO.File.ReadAllLines(absolutePath);

            foreach (var line in lines.Skip(1))
            {
                var parts = line.Split(',');
                if (parts.Length >= 2)
                {
                    doctors.Add(new DoctorCsvRecord
                    {
                        Name = parts[0].Trim(),
                        Specialization = string.Join(",", parts.Skip(1)).Trim() // In case specialization has commas
                    });
                }
            }

            return doctors;
        }

        private List<CityCoordinates> GetEgyptianCities()
        {
            return new List<CityCoordinates>
            {
                new CityCoordinates { Name = "Cairo", Latitude = 30.0444, Longitude = 31.2357 },
                new CityCoordinates { Name = "Alexandria", Latitude = 31.2001, Longitude = 29.9187 },
                new CityCoordinates { Name = "Giza", Latitude = 30.0131, Longitude = 31.2089 },
                new CityCoordinates { Name = "Shubra El-Kheima", Latitude = 30.1286, Longitude = 31.2422 },
                new CityCoordinates { Name = "Port Said", Latitude = 31.2565, Longitude = 32.2841 },
                new CityCoordinates { Name = "Suez", Latitude = 29.9668, Longitude = 32.5498 },
                new CityCoordinates { Name = "Mansoura", Latitude = 31.0409, Longitude = 31.3785 },
                new CityCoordinates { Name = "El Mahalla El Kubra", Latitude = 30.9706, Longitude = 31.1669 },
                new CityCoordinates { Name = "Tanta", Latitude = 30.7865, Longitude = 31.0004 },
                new CityCoordinates { Name = "Asyut", Latitude = 27.1810, Longitude = 31.1837 },
                new CityCoordinates { Name = "Ismailia", Latitude = 30.5965, Longitude = 32.2715 },
                new CityCoordinates { Name = "Faiyum", Latitude = 29.3099, Longitude = 30.8418 },
                new CityCoordinates { Name = "Zagazig", Latitude = 30.5877, Longitude = 31.5020 },
                new CityCoordinates { Name = "Aswan", Latitude = 24.0889, Longitude = 32.8998 },
                new CityCoordinates { Name = "Damietta", Latitude = 31.4165, Longitude = 31.8133 },
                new CityCoordinates { Name = "Damanhur", Latitude = 31.0414, Longitude = 30.4658 },
                new CityCoordinates { Name = "Minya", Latitude = 28.1099, Longitude = 30.7503 },
                new CityCoordinates { Name = "Beni Suef", Latitude = 29.0661, Longitude = 31.0994 },
                new CityCoordinates { Name = "Qena", Latitude = 26.1615, Longitude = 32.7151 },
                new CityCoordinates { Name = "Sohag", Latitude = 26.5570, Longitude = 31.6948 },
                new CityCoordinates { Name = "Hurghada", Latitude = 27.2579, Longitude = 33.8116 },
                new CityCoordinates { Name = "Banha", Latitude = 30.4660, Longitude = 31.1793 },
                new CityCoordinates { Name = "Kafr El Sheikh", Latitude = 31.1107, Longitude = 30.9388 },
                new CityCoordinates { Name = "Arish", Latitude = 31.1316, Longitude = 33.7984 },
                new CityCoordinates { Name = "Mallawi", Latitude = 27.7314, Longitude = 30.8354 },
                new CityCoordinates { Name = "10th of Ramadan", Latitude = 30.3015, Longitude = 31.7406 },
                new CityCoordinates { Name = "Luxor", Latitude = 25.6872, Longitude = 32.6396 }
            };
        }
    }

    public class DoctorCsvRecord
    {
        public string Name { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
    }

    public class CityCoordinates
    {
        public string Name { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
