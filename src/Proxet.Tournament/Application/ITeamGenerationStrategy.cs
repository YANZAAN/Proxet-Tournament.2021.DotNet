using System.Collections.Generic;
using Proxet.Tournament.Domain;

namespace Proxet.Tournament.Application
{
    public interface ITeamGenerationStrategy
    {
        (string[] team1, string[] team2) Generate(IEnumerable<UsernameWaitingProfile> players);
    }
}