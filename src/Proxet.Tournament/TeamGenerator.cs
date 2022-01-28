using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Proxet.Tournament
{
    public class TeamGenerator
    {
        public (string[] team1, string[] team2) GenerateTeams(string filePath)
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

            var orderedPlayers = players.OrderByDescending(player => player.WaitingTime);

            var takeCount = 24; // > 9*2
            var playersPool = new ConcurrentBag<UsernameWaitingProfile>();
            var playersBucket = orderedPlayers.Take(takeCount)
                                              .ToArray()
                                              .AsEnumerable();

            Parallel
                .For(1, 4,
                     new ParallelOptions() { MaxDegreeOfParallelism = 3 },
                     counter =>
                     {
                         AddToConcurrentBag(
                             playersPool,
                             playersBucket.Where(player => player.VehicleClass == counter)
                                          .Take(6)
                                          .ToArray()
                         );
                     }
            );

            if (playersPool.Count == 18)
            {
                return EvenAndOddPartitionOf(
                    playersPool.Select(player => player.Username)
                );
            }

            /**
             * Additional looping if final count wasn't found (no sense in multithreading)
             */
            var skipCount = takeCount;
            takeCount = 6;
            var teams = playersPool.GroupBy(player => player.VehicleClass)
                                   .ToDictionary(g => g.Key, g => g.ToList());

            do
            {
                playersBucket = orderedPlayers.Skip(skipCount)
                                              .Take(takeCount);

                foreach (var player in playersBucket)
                {
                    if (!teams.ContainsKey(player.VehicleClass))
                    {
                        teams.Add(player.VehicleClass, new List<UsernameWaitingProfile>() { player });
                        continue;
                    }

                    if (teams[player.VehicleClass].Count < 6)
                    {
                        teams[player.VehicleClass].Add(player);
                    }
                }

                skipCount += takeCount;
            }
            while (teams.Values.Sum(list => list.Count) != 18);

            return EvenAndOddPartitionOf(
                teams.Values.SelectMany(x => x)
                            .ToArray()
                            .Select(player => player.Username)
            );
        }

        private static void AddToConcurrentBag(ConcurrentBag<UsernameWaitingProfile> bag, IEnumerable<UsernameWaitingProfile> list)
        {
            for (var i = 0; i < list.Count(); i++)
            {
                bag.Add(list.ElementAt(i));
            }
        }

        private (string[] team1, string[] team2) EvenAndOddPartitionOf(IEnumerable<string> names)
        =>
        (
            names.Where((_, index) => index % 2 != 0).ToArray(),
            names.Where((_, index) => index % 2 == 0).ToArray()
        );

    }
}