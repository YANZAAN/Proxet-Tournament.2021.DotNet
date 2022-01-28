using System.Collections.Generic;
using System.Linq;

namespace Proxet.Tournament
{
    public class StandartGenerationStrategy : ITeamGenerationStrategy
    {
        public (string[] team1, string[] team2) Generate(IEnumerable<UsernameWaitingProfile> players)
        {
            return (Enumerable.Empty<string>().ToArray(), Enumerable.Empty<string>().ToArray());          
        }
    }
}