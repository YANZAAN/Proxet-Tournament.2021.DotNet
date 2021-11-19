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
            var players = from row in System.IO.File.ReadLines(filePath).Skip(1)
                          let unseparated = row.Split('\t')
                          select new TeamEntry
                          {
                              Username = unseparated[0],
                              WaitingTime = ushort.Parse(unseparated[1]),
                              VehicleClass = byte.Parse(unseparated[2])
                          };

            /**
             * 1. Longest waiting time (should import all rows to analyze though)
             */
            var orderedPlayers = players.OrderByDescending(player => player.WaitingTime);

            var firstLoopBuffer = 24; // > 9*2
            var mainBucket = orderedPlayers.Take(firstLoopBuffer)
                                           .ToList();
            var concurrentPool = new System.Collections.Concurrent.ConcurrentBag<TeamEntry>();
            /**
             * 2. First multithread loop (some boost acquired)
             */
            Parallel
                .For(1, 4,
                     new ParallelOptions() { MaxDegreeOfParallelism = 3 },
                     counter =>
                     {
                         mainBucket.Where(player => player.VehicleClass == counter)
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
            var skipIndex = firstLoopBuffer;
            var takeIndex = 9;
            var playersBucket = default(IEnumerable<TeamEntry>);
            var teams = concurrentPool.GroupBy(player => player.VehicleClass)
                                      .ToDictionary(g => g.Key, g => g.ToList());
            do
            {
                playersBucket = orderedPlayers.Skip(skipIndex).Take(takeIndex);

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

                skipIndex += takeIndex;
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