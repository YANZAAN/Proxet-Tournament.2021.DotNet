using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Proxet.Tournament
{
    public class TeamGenerator
    {
        private ITeamGenerationStrategy _generationStrategy;

        public TeamGenerator SetGenerationStrategy(ITeamGenerationStrategy generationStrategy)
        {
            _generationStrategy = generationStrategy;
            return this;
        }

        public (string[] team1, string[] team2) GenerateTeams(string filePath, ITeamGenerationStrategy preferableGenerationStrategy = default)
        {
            var players = System.IO.File.ReadLines(filePath)
                .Skip(1)
                .Select(row => row.Split('\t'))
                .Select(list => new UsernameWaitingProfile
                {
                    Username = list[0],
                    WaitingTime = int.Parse(list[1]),
                    VehicleClass = int.Parse(list[2])
                });

            var generationStrategy = preferableGenerationStrategy ??
                _generationStrategy ??
                new ParallelGenerationStrategy();

            var teams = generationStrategy.Generate(players);

            return teams;
        }
    }
}