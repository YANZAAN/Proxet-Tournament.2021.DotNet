using System.IO;
using System.Linq;

using FluentAssertions;
using Xunit;

namespace Proxet.Tournament
{
    public class TournamentTests
    {
        private const string StatFile = "wait-time.stat";

        private readonly string[] _class1Users =
        {
            "_{Basil$@",
            "o-Ben#-",
            "+oBarney_+",
            "#%Gerard@%",
            "-@Grant]%",
            "<OEmil%#",
        };

        private readonly string[] _class2Users =
        {
            "O$Mickey$@",
            "$-Dereko-",
            "o@Rusty#*",
            "^[Andres_$",
            "*<Tomas$_",
            "##Dwayne_o",
        };

        private readonly string[] _class3Users =
        {
            "O#Truman#@",
            "*%Wesley-+",
            "-{Sidney_o",
            "[#Andre]O",
            "-[Oliver}@",
            "^[Gene*+",
        };

        [Fact]
        public void ItEnsuresThatFileIsPresent()
        {
            var s = File.Exists(StatFile);
            s.Should().BeTrue();
        }

        [Fact]
        public void ItEnsuresThatIdealTeamsWasReturned()
        {
            //arrange
            var generator = new TeamGenerator();

            //act
            var teams = generator.GenerateTeams(StatFile);

            //assert
            teams.team1.Length.Should().Be(9, "Team 1 should contain 9 players");
            teams.team2.Length.Should().Be(9, "Team 2 should contain 9 players");
            teams.team1.Intersect(teams.team2).Count().Should().Be(0, "Teams should not include the same players");

            AssertClass(teams.team1, teams.team2, _class1Users, 1);
            AssertClass(teams.team1, teams.team2, _class2Users, 2);
            AssertClass(teams.team1, teams.team2, _class3Users, 3);
        }

        private void AssertClass(string[] team1, string[] team2, string[] classUsers, int vehicleClass)
        {
            team1.Intersect(classUsers).Count().Should()
                .Be(3, $"Team 1 should contain 3 players with most wait time of vehicle class {vehicleClass}");
            team2.Intersect(classUsers).Count().Should()
                .Be(3, $"Team 2 should contain 3 players with most wait time of vehicle class {vehicleClass}");
        }
    }
}