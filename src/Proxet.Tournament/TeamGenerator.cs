using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Proxet.Tournament
{
    public class TeamEntry
    {
        public string Username;
        public ushort WaitingTime;
        public byte VehicleClass;
    }

    public class TeamGenerator
    {
        public (string[] team1, string[] team2) GenerateTeams(string filePath)
        {
            /**
             * 0. Reading from file
             * Retrievement to hashtable? With hash based on waiting_time's standart deviation?
             * Partial retrievement?
             */
            var players = System.IO.File.ReadLines(filePath)
                                        .Skip(1)
                                        .Select(row => row.Split('\t'))
                                        .Select(entityArray => new TeamEntry
                                        {
                                            Username = entityArray[0],
                                            WaitingTime = ushort.Parse(entityArray[1]),
                                            VehicleClass = byte.Parse(entityArray[2])
                                        });

            /**
             * 1. Longest waiting time (should import all rows to analyze though)
             */
            var orderedPlayers = players.OrderByDescending(player => player.WaitingTime);

            var takeCount = 24; // > 9*2
            var concurrentPool = new System.Collections.Concurrent.ConcurrentBag<TeamEntry>();
            var playersBucket = orderedPlayers.Take(takeCount)
                                              .ToList()
                                              .AsEnumerable();
            /**
             * 2. First multithread loop (some boost acquired)
             */
            Parallel
                .For(1, 4,
                     new ParallelOptions() { MaxDegreeOfParallelism = 3 },
                     counter =>
                     {
                         playersBucket.Where(player => player.VehicleClass == counter)
                                      .Take(6)
                                      .ToList()
                                      .ForEach(player => concurrentPool.Add(player));
                     }
            );

            var playersPool = default(IEnumerable<string>);
            if (concurrentPool.Count == 18)
            {
                playersPool = concurrentPool.Select(player => player.Username);

                return EvenAndOddPartitionOf(playersPool);
            }

            /**
             * 3. Additional looping if final count wasn't found (no sense in multithreading)
             */
            var skipCount = takeCount;
            takeCount = 9;
            var teams = concurrentPool.GroupBy(player => player.VehicleClass)
                                      .ToDictionary(g => g.Key, g => g.ToList());

            do
            {
                playersBucket = orderedPlayers.Skip(skipCount)
                                              .Take(takeCount);

                foreach (var player in playersBucket)
                {
                    if (!teams.ContainsKey(player.VehicleClass))
                    {
                        teams.Add(player.VehicleClass, new List<TeamEntry>() { player });
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

            playersPool = teams.Values.SelectMany(x => x)
                                      .ToList()
                                      .Select(player => player.Username);

            return EvenAndOddPartitionOf(playersPool);
        }

        public (string[] team1, string[] team2) EvenAndOddPartitionOf(IEnumerable<string> names)
        =>
        (
            names.Where((_, index) => index % 2 != 0).ToArray(),
            names.Where((_, index) => index % 2 == 0).ToArray()
        );
    }
}