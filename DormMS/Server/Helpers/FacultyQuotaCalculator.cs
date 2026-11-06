namespace DormMS.Server.Helpers;
public static class FacultyQuotaCalculator
{
    public static Dictionary<string, int> Calculate(
        Dictionary<string, int> facultyApplicationCounts,
        int maxStudents)
    {
        var quotas = new Dictionary<string, int>();

        var total = facultyApplicationCounts.Values.Sum();
        if (total == 0)
            return quotas;

        var assigned = 0;

        foreach (var faculty in facultyApplicationCounts)
        {
            var proportion = (double)faculty.Value / total;
            var quota = (int)Math.Floor(proportion * maxStudents);
            quotas[faculty.Key] = quota;
            assigned += quota;
        }

        // Ostatak zbog Floor zaokruzivanja - dodeli fakultetima sa najvise prijava
        var remaining = maxStudents - assigned;
        var sortedBySize = facultyApplicationCounts
            .OrderByDescending(f => f.Value)
            .Select(f => f.Key)
            .ToList();

        for (int i = 0; i < remaining; i++)
            quotas[sortedBySize[i % sortedBySize.Count]]++;

        return quotas;
    }
}
