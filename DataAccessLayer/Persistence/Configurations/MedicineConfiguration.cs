using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DataAccessLayer.Persistence.Configurations
{
    public class MedicineConfiguration : IEntityTypeConfiguration<Medicine>
    {
        public void Configure(EntityTypeBuilder<Medicine> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Id)
                .ValueGeneratedOnAdd();

            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(m => m.ArabicName)
                .HasMaxLength(300);

            builder.Property(m => m.ArabicNameNormalized)
                .HasMaxLength(300);

            builder.Property(m => m.Price)
                .HasMaxLength(50);

            builder.Property(m => m.Company)
                .HasMaxLength(300);

            builder.Property(m => m.ActiveIngredient); // nvarchar(max)

            builder.Property(m => m.Description)
                .HasMaxLength(2000);

            builder.Property(m => m.ProductUrl)
                .HasMaxLength(500);

            builder.HasIndex(m => m.Name)
                .HasDatabaseName("IX_Medicines_Name");

            builder.HasIndex(m => m.ArabicNameNormalized)
                .HasDatabaseName("IX_Medicines_ArabicNameNormalized");
        }
    }
}
