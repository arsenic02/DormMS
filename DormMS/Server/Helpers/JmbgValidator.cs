public static class JmbgValidator
{
    public static bool IsValid(string jmbg)
    {
        if (string.IsNullOrWhiteSpace(jmbg) || jmbg.Length != 13 || !jmbg.All(char.IsDigit))
            return false;

        int[] digits = jmbg.Select(c => c - '0').ToArray();

        int sum = 7 * (digits[0] + digits[6])
                + 6 * (digits[1] + digits[7])
                + 5 * (digits[2] + digits[8])
                + 4 * (digits[3] + digits[9])
                + 3 * (digits[4] + digits[10])
                + 2 * (digits[5] + digits[11]);

        int remainder = sum % 11;
        int control = remainder < 2 ? remainder : 11 - remainder;

        return control == digits[12];
    }

    public static DateTime? ExtractBirthDate(string jmbg)
    {
        if (!IsValid(jmbg)) return null;
        
        int day = int.Parse(jmbg[..2]);
        int month = int.Parse(jmbg[2..4]);
        int yearSuffix = int.Parse(jmbg[4..7]);
        // 000-899 → 2000-2899, 900-999 → 1900-1999
        int year = yearSuffix >= 900 ? 1000 + yearSuffix : 2000 + yearSuffix;

        try { return new DateTime(year, month, day); }
        catch { return null; }
    }
}