using System.Globalization;
using ELECTRICITRACK.Models;

namespace ELECTRICITRACK.Services;

public static class ConsumptionCalculator
{
    public static decimal MonthlyKwh(Appliance appliance) =>
        appliance.PowerWatts / 1000m * appliance.HoursPerDay * appliance.DaysPerMonth;

    public static decimal MonthlyCost(Appliance appliance, decimal ratePerKwh) =>
        MonthlyKwh(appliance) * ratePerKwh;

    public static string Money(decimal amount) =>
        $"₱{amount.ToString("N2", CultureInfo.InvariantCulture)}";
}

public class ConsumptionAnalysisService
{
    public List<Appliance> SortByMonthlyConsumption(IEnumerable<Appliance> appliances)
    {
        var result = appliances.ToArray();
        if (result.Length < 2)
        {
            return result.ToList();
        }

        for (var root = result.Length / 2 - 1; root >= 0; root--)
        {
            SiftDown(result, root, result.Length);
        }

        for (var end = result.Length - 1; end > 0; end--)
        {
            (result[0], result[end]) = (result[end], result[0]);
            SiftDown(result, 0, end);
        }

        Array.Reverse(result);
        return result.ToList();
    }

    public List<Appliance> SearchByNamePrefix(IEnumerable<Appliance> appliances, string? search)
    {
        var sorted = appliances.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase).ToArray();
        var prefix = search?.Trim();
        if (string.IsNullOrEmpty(prefix))
        {
            return sorted.ToList();
        }

        var low = 0;
        var high = sorted.Length;
        while (low < high)
        {
            var middle = low + (high - low) / 2;
            var comparison = sorted[middle].Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? 0
                : string.Compare(sorted[middle].Name, prefix, StringComparison.OrdinalIgnoreCase);
            if (comparison < 0)
            {
                low = middle + 1;
            }
            else
            {
                high = middle;
            }
        }

        var matches = new List<Appliance>();
        for (var index = low; index < sorted.Length &&
             sorted[index].Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase); index++)
        {
            matches.Add(sorted[index]);
        }

        return matches;
    }

    private static void SiftDown(Appliance[] items, int root, int count)
    {
        while (true)
        {
            var largest = root;
            var left = root * 2 + 1;
            var right = left + 1;

            if (left < count && CompareConsumption(items[left], items[largest]) > 0)
            {
                largest = left;
            }

            if (right < count && CompareConsumption(items[right], items[largest]) > 0)
            {
                largest = right;
            }

            if (largest == root)
            {
                return;
            }

            (items[root], items[largest]) = (items[largest], items[root]);
            root = largest;
        }
    }

    private static int CompareConsumption(Appliance left, Appliance right)
    {
        var consumptionComparison = ConsumptionCalculator.MonthlyKwh(left)
            .CompareTo(ConsumptionCalculator.MonthlyKwh(right));
        return consumptionComparison != 0
            ? consumptionComparison
            : string.Compare(left.Name, right.Name, StringComparison.OrdinalIgnoreCase);
    }
}
