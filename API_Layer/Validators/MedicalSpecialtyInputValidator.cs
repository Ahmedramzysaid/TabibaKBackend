using System.Text.RegularExpressions;

namespace ClinicAPI.Validators;

public static class MedicalSpecialtyInputValidator
{
    private static readonly Regex LinkPattern = new(@"https?://|www\.", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly Regex RepetitivePattern = new(@"(.)\1{4,}", RegexOptions.Compiled); // same char 5+ times
    private static readonly Regex EnglishWordPattern = new(@"\b[a-zA-Z]{3,}\b", RegexOptions.Compiled);

    private const double MaxNumericDensity = 0.30;
    private const string Vowels = "aeiouAEIOU";

    public static (bool IsValid, string? ErrorCode, string? Message) Validate(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return (false, "REQUIRED", "Text is required");

        var t = text.Trim();
        if (t.Length < 10)
            return (false, "TOO_SHORT", "Text must be at least 10 characters");
        if (t.Length > 500)
            return (false, "TOO_LONG", "Text must be at most 500 characters");

        if (LinkPattern.IsMatch(t))
            return (false, "LINK_DETECTED", "Invalid input: links are not allowed");

        if (RepetitivePattern.IsMatch(t))
            return (false, "REPETITIVE_PATTERN", "Invalid input: repetitive character patterns are not allowed");

        var digitCount = t.Count(char.IsDigit);
        if (t.Length > 0 && (double)digitCount / t.Length > MaxNumericDensity)
            return (false, "NUMERIC_DENSITY", "Invalid input: digits must not exceed 30% of the text");

        if (!IsAllowedLanguage(t))
            return (false, "LANGUAGE_NOT_SUPPORTED", "Invalid input: only Arabic and English are supported");

        if (ContainsGibberish(t))
            return (false, "GIBBERISH_DETECTED", "Invalid input: text appears to be gibberish or invalid");

        return (true, null, null);
    }

    private static bool IsAllowedLanguage(string text)
    {
        var normalized = text.Normalize();
        foreach (var c in normalized)
        {
            if (char.IsWhiteSpace(c) || char.IsPunctuation(c) || char.IsSymbol(c))
                continue;
            if (char.IsDigit(c))
                continue;
            var isArabic = c >= '\u0600' && c <= '\u06FF' || c >= '\u0750' && c <= '\u08FF';
            var isLatin = c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z';
            if (!isArabic && !isLatin)
                return false;
        }
        return true;
    }

    private static bool ContainsGibberish(string text)
    {
        var englishWords = EnglishWordPattern.Matches(text).Select(m => m.Value).ToList();
        if (englishWords.Count == 0)
            return false;
        var wordsWithoutVowel = englishWords.Where(w => w.Length >= 2 && !w.Any(ch => Vowels.Contains(ch))).ToList();
        if (wordsWithoutVowel.Count >= 2)
            return true;
        var mostlySymbols = text.Count(c => !char.IsLetterOrDigit(c) && !char.IsWhiteSpace(c));
        if (text.Length > 0 && (double)mostlySymbols / text.Length > 0.5)
            return true;
        return false;
    }
}
