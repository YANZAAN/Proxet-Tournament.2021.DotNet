using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Proxet.Tournament.Domain;
using Proxet.Tournament.Utility;

namespace Proxet.Tournament.Application
{
    public class ParallelGenerationStrategy : ITeamGenerationStrategy
    {
        /// <summary>
        ///     Initial player chunk retrieved in parallel
        /// </summary>
        private int _initialChunkSize;
        /// <summary>
        ///     Subsequent chunk size for additions if parallel extraction result wasn't enough
        /// </summary>
        private int _bufferChunkSize;
        public ParallelGenerationStrategy(int initialChunkSize = 24, int bufferChunkSize = 6)
        {
            _initialChunkSize = initialChunkSize;
            _bufferChunkSize = bufferChunkSize;
        }

        public (string[] team1, string[] team2) Generate(IEnumerable<UsernameWaitingProfile> players)
        {
            var orderedPlayers = players.OrderByDescending(player => player.WaitingTime);
            var playersPool = new ConcurrentBag<UsernameWaitingProfile>();
            var playersBucket = orderedPlayers.Take(_initialChunkSize)
                .ToArray()
                .AsEnumerable();

            Parallel.For(1, 4, new ParallelOptions() { MaxDegreeOfParallelism = 3 },
                counter =>
                {
                    var bucket = playersBucket
                        .Where(player => player.VehicleClass == counter)
                        .Take(6)
                        .ToArray();

                    foreach (var player in bucket)
                    {
                        playersPool.Add(player);
                    }
                }
            );

            if (playersPool.Count == 18)
            {
                return playersPool.Select(player => player.Username)
                    .GetEvenAndOddPartition();
            }

            var skipCount = _initialChunkSize;
            var teams = playersPool.GroupBy(player => player.VehicleClass)
                .ToDictionary(g => g.Key, g => g.ToList());

            do
            {
                playersBucket = orderedPlayers.Skip(skipCount)
                    .Take(_bufferChunkSize);

                foreach (var player in playersBucket)
                {
                    if (teams[player.VehicleClass].Count < 6)
                    {
                        teams[player.VehicleClass].Add(player);
                    }
                }

                skipCount += _bufferChunkSize;
            }
            while (teams.Values.Sum(list => list.Count) != 18);

            return teams.Values.SelectMany(x => x)
                .Select(player => player.Username)
                .GetEvenAndOddPartition();
        }
    }
}