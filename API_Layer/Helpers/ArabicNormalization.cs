using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace ClinicAPI.Helpers;

public static class ArabicNormalization
{
    private static readonly Regex TashkeelRegex = new(@"[\u064B-\u0652\u0670]", RegexOptions.Compiled);

    public static string Normalize(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var sb = new StringBuilder(text.Length);
        foreach (var c in text.Normalize(NormalizationForm.FormC))
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark) // Tashkeel
                continue;
            var normalized = c switch
            {
                'أ' or 'إ' or 'آ' => 'ا',
                'ى' => 'ي',
                'ة' => 'ه',
                'ـ' => (char?)null, // Tatweel (kashida) - skip
                '٠' => '0', '١' => '1', '٢' => '2', '٣' => '3', '٤' => '4', '٥' => '5', '٦' => '6', '٧' => '7', '٨' => '8', '٩' => '9', // Arabic-Indic digits
                _ => c
            };
            if (normalized.HasValue)
                sb.Append(normalized.Value);
        }

        var result = sb.ToString();
        result = TashkeelRegex.Replace(result, string.Empty);
        return result.Trim();
    }
}
