using AIPrototypeAssetRepair.Models;
using System.Text.RegularExpressions;

namespace AIPrototypeAssetRepair
{
    namespace AIPrototypeAssetRepair.Helper
    {
        public static class SimilarityHelper
        {
            public static List<RepairLog> FindMostSimilarLogs(
                string currentFailureDescription,
                List<RepairLog> repairLogs,
                int topN = 3)
            {
                var cleanedCurrent = CleanText(currentFailureDescription);

                var scored = repairLogs
                    .Select(log => new
                    {
                        Log = log,
                        Score = JaccardSimilarity(cleanedCurrent, CleanText(log.Notes ?? ""))
                    })
                    .OrderByDescending(x => x.Score)
                    .Take(topN)
                    .Select(x => x.Log)
                    .ToList();

                return scored;
            }

            private static string CleanText(string text)
            {
                text = text.ToLowerInvariant();
                text = Regex.Replace(text, @"[^\w\s]", ""); // remove punctuation
                return text;
            }

            private static double JaccardSimilarity(string text1, string text2)
            {
                var set1 = new HashSet<string>(text1.Split(' ', StringSplitOptions.RemoveEmptyEntries));
                var set2 = new HashSet<string>(text2.Split(' ', StringSplitOptions.RemoveEmptyEntries));

                var intersection = set1.Intersect(set2).Count();
                var union = set1.Union(set2).Count();

                return union == 0 ? 0 : (double)intersection / union;
            }
        }
    }

}
