using System;

namespace Proxet.Tournament
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new TeamGenerator();
            var teams = generator.GenerateTeams("wait-time.stat");

            OutputTeam("Red", teams.team1);
            OutputTeam("Blue", teams.team2);
            Console.WriteLine("Work is done");
        }

        private static void OutputTeam(string name, string[] users)
        {
            Console.WriteLine($"Team {name}:");
            foreach (var user in users)
            {
                Console.WriteLine(user);
            }
        }
    }
}