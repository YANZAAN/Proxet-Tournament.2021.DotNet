using System.Collections.Generic;
using System.Linq;

namespace Proxet.Tournament
{
    public class StandartGenerationStrategy : ITeamGenerationStrategy
    {
        public (string[] team1, string[] team2) Generate(IEnumerable<UsernameWaitingProfile> players)
        {
            var orderedPlayers = players.OrderBy(p => p.VehicleClass)
                .ThenByDescending(p => p.WaitingTime);

            var teams = orderedPlayers.GroupBy(player => player.VehicleClass)
                .ToDictionary(g => g.Key, g => g.Take(6).ToList());

            return teams.Values.SelectMany(x => x)
                .Select(player => player.Username)
                .GetEvenAndOddPartition();      
        }
    }
}