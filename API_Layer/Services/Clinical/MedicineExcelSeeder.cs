using ClosedXML.Excel;
using ClinicAPI.Helpers;
using DataAccessLayer.Persistence;
using DomainLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace ClinicAPI.Services;

public static class MedicineExcelSeeder
{
    public static async Task SeedAsync(ApplicationDbContext dbContext, string excelPath, ILogger logger)
    {
        if (await dbContext.Medicines.AnyAsync())
        {
            var count = await dbContext.Medicines.CountAsync();
            logger.LogInformation("✅ Medicines table already has {Count} records — skipping Excel seed.", count);
            return;
        }

        if (!File.Exists(excelPath))
        {
            logger.LogWarning("⚠️ Medicines Excel file not found: {Path}. Medicines table will be empty.", excelPath);
            return;
        }

        try
        {
            using var workbook = new XLWorkbook(excelPath);
            var sheet = workbook.Worksheet(1);
            var rows = sheet.RowsUsed().Skip(1).ToList();

            if (rows.Count == 0)
            {
                logger.LogWarning("⚠️ Medicines Excel has no data rows. File: {Path}", excelPath);
                return;
            }

            var header = sheet.Row(1);
            int colCount = Math.Max(sheet.LastColumnUsed()?.ColumnNumber() ?? 0, 7);
            int nameCol = 0, arabicNameCol = 0, priceCol = 0, companyCol = 0,
                activeIngredientCol = 0, descriptionCol = 0, productUrlCol = 0;

            for (int c = 1; c <= colCount; c++)
            {
                var cell = header.Cell(c).GetString().Trim();
                if (string.IsNullOrEmpty(cell)) continue;
                var lower = cell.ToLowerInvariant();

                if (nameCol == 0 && (lower == "name" || lower == "trade name" || lower == "english name" || lower == "product name")) nameCol = c;
                if (arabicNameCol == 0 && (lower == "arabic_name" || lower == "name_arabic" || lower == "arabic name" || lower.Contains("arabic"))) arabicNameCol = c;
                if (activeIngredientCol == 0 && (lower == "active_ingredient" || lower == "active ingredient")) activeIngredientCol = c;
                if (priceCol == 0 && lower.Contains("price")) priceCol = c;
                if (companyCol == 0 && lower.Contains("company")) companyCol = c;
                if (descriptionCol == 0 && (lower == "description" || lower == "desc")) descriptionCol = c;
                if (productUrlCol == 0 && (lower == "product_url" || lower == "product url" || lower == "url")) productUrlCol = c;
            }

            if (nameCol == 0 && colCount >= 1) nameCol = 1;
            if (arabicNameCol == 0 && colCount >= 2) arabicNameCol = 2;
            if (activeIngredientCol == 0 && colCount >= 3) activeIngredientCol = 3;
            if (priceCol == 0 && colCount >= 4) priceCol = 4;
            if (companyCol == 0 && colCount >= 5) companyCol = 5;

            var medicines = new List<Medicine>();
            foreach (var row in rows)
            {
                var name = nameCol > 0 ? row.Cell(nameCol).GetString().Trim() : "";
                var arabicName = arabicNameCol > 0 ? row.Cell(arabicNameCol).GetString().Trim() : "";
                var price = priceCol > 0 ? row.Cell(priceCol).GetString().Trim() : null;
                if (priceCol > 0 && string.IsNullOrEmpty(price) && row.Cell(priceCol).TryGetValue(out double numVal))
                    price = numVal.ToString();
                var company = companyCol > 0 ? row.Cell(companyCol).GetString().Trim() : null;
                var activeIngredient = activeIngredientCol > 0 ? row.Cell(activeIngredientCol).GetString().Trim() : null;
                var description = descriptionCol > 0 ? row.Cell(descriptionCol).GetString().Trim() : null;
                var productUrl = productUrlCol > 0 ? row.Cell(productUrlCol).GetString().Trim() : null;

                if (string.IsNullOrEmpty(name)) name = "";
                if (string.IsNullOrEmpty(arabicName)) arabicName = "";

                medicines.Add(new Medicine
                {
                    Name = name,
                    ArabicName = arabicName,
                    ArabicNameNormalized = ArabicNormalization.Normalize(arabicName),
                    Price = string.IsNullOrEmpty(price ?? "") ? null : price,
                    Company = string.IsNullOrEmpty(company ?? "") ? null : company,
                    ActiveIngredient = string.IsNullOrEmpty(activeIngredient ?? "") ? null : activeIngredient,
                    Description = string.IsNullOrEmpty(description ?? "") ? null : description,
                    ProductUrl = string.IsNullOrEmpty(productUrl ?? "") ? null : productUrl
                });
            }

            await dbContext.Medicines.AddRangeAsync(medicines);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("✅ Seeded {Count} medicines from Excel into database. File: {Path}", medicines.Count, excelPath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to seed medicines from Excel: {Path}", excelPath);
        }
    }
}
