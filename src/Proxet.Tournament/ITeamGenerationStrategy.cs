using System.Collections.Generic;

namespace Proxet.Tournament
{
    public interface ITeamGenerationStrategy
    {
        (string[] team1, string[] team2) Generate(IEnumerable<UsernameWaitingProfile> players);
    }
}