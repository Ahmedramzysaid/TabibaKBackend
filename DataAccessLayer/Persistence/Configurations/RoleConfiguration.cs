using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<IdentityRole>
{
    public void Configure(EntityTypeBuilder<IdentityRole> builder)
    {
        builder.HasData(
            new List<IdentityRole>
            {
                new()
                {
                    Id = "d1f488a3-6730-47cb-a0e1-aaa2342a1bc1",
                    Name = "SuperAdmin",
                    NormalizedName = "SUPERADMIN",
                    ConcurrencyStamp = "cf7c214e-6bc4-4ef5-9cd6-111122223333"
                },
                new()
                {
                    Id = "f6a822c6-2ad8-4d32-a0d1-eee23453bc22",
                    Name = "ClinicManager",
                    NormalizedName = "CLINICMANAGER",
                    ConcurrencyStamp = "442899dd-c394-4a76-87f0-222233334444"
                },
                new()
                {
                    Id = "aa19c2e5-3de4-471e-a31e-bbb344567899",
                    Name = "MedicalAdmin",
                    NormalizedName = "MEDICALADMIN",
                    ConcurrencyStamp = "efcc233c-cffa-4777-8d15-333344445555"
                },
                new()
                {
                    Id = "bb95a2dc-cba4-4fcb-9432-ccc455667788",
                    Name = "Receptionist",
                    NormalizedName = "RECEPTIONIST",
                    ConcurrencyStamp = "aaad1111-bbdd-4567-ae88-444455556666"
                },
                new()
                {
                    Id = "cc88a3ed-dcb5-5fdc-a543-ddd566778899",
                    Name = "Doctor",
                    NormalizedName = "DOCTOR",
                    ConcurrencyStamp = "bbbe2222-ccdd-5678-af99-555566667777"
                },
                new()
                {
                    Id = "dd99b4fe-edc6-6ged-b654-eee677889900",
                    Name = "Patient",
                    NormalizedName = "PATIENT",
                    ConcurrencyStamp = "cccf3333-ddee-6789-ag00-666677778888"
                }
            });
    }
}
