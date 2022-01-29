using System.Collections.Generic;
using System.Linq;

namespace Proxet.Tournament.Utility
{
    public static class UtilityExtensions
    {
        public static (string[] team1, string[] team2) GetEvenAndOddPartition(this IEnumerable<string> names)
        =>
        (
            names.Where((_, index) => index % 2 != 0).ToArray(),
            names.Where((_, index) => index % 2 == 0).ToArray()
        );
    }
}