using OGPLib;
using pacman;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server {


    public class StateGenerator {
        public static PacmanState GenerateDefaultState() {
            List<Player> players = new List<Player>();
            List<Wall> walls = CreateDefaultWalls();
            List<Coin> coins = CreateDefaultCoins();
            List<Monster> monsters = CreateDefaultMonsters();
            return new PacmanState(monsters, coins, players, walls, 0);

        }

        public static List<Monster> CreateDefaultMonsters() {
            List<Monster> monsters = new List<Monster>();

            // Create colored monsters
            monsters.Add(new Monster(new Coordinate2D(301, 72), Monster.MonsterColor.Pink, Direction.RIGHTUP));
            monsters.Add(new Monster(new Coordinate2D(221, 273), Monster.MonsterColor.Yellow, Direction.UP));
            monsters.Add(new Monster(new Coordinate2D(180, 73), Monster.MonsterColor.Red, Direction.LEFT));

            return monsters;
        }

        public static List<Wall> CreateDefaultWalls() {
            List<Wall> walls = new List<Wall>();

            walls.Add(new Wall(new Coordinate2D(88, 40), 15, 95));
            walls.Add(new Wall(new Coordinate2D(248, 40), 15, 95));
            walls.Add(new Wall(new Coordinate2D(128, 240), 15, 95));
            walls.Add(new Wall(new Coordinate2D(288, 240), 15, 95));

            return walls;
        }

        public static List<Coin> CreateDefaultCoins() {
            List<Coin> coins = new List<Coin>();

            coins.Add(new Coin(new Coordinate2D(11, 49)));
            coins.Add(new Coin(new Coordinate2D(11, 98)));
            coins.Add(new Coin(new Coordinate2D(11, 148)));
            coins.Add(new Coin(new Coordinate2D(11, 197)));
            coins.Add(new Coin(new Coordinate2D(11, 246)));
            coins.Add(new Coin(new Coordinate2D(11, 295)));
            coins.Add(new Coin(new Coordinate2D(11, 345)));
            coins.Add(new Coin(new Coordinate2D(11, 394)));

            coins.Add(new Coin(new Coordinate2D(64, 49)));
            coins.Add(new Coin(new Coordinate2D(64, 98)));
            coins.Add(new Coin(new Coordinate2D(64, 148)));
            coins.Add(new Coin(new Coordinate2D(64, 197)));
            coins.Add(new Coin(new Coordinate2D(64, 246)));
            coins.Add(new Coin(new Coordinate2D(64, 295)));
            coins.Add(new Coin(new Coordinate2D(64, 345)));
            coins.Add(new Coin(new Coordinate2D(64, 394)));

            coins.Add(new Coin(new Coordinate2D(117, 197)));
            coins.Add(new Coin(new Coordinate2D(117, 246)));
            coins.Add(new Coin(new Coordinate2D(117, 295)));
            coins.Add(new Coin(new Coordinate2D(117, 345)));
            coins.Add(new Coin(new Coordinate2D(117, 394)));

            coins.Add(new Coin(new Coordinate2D(171, 49)));
            coins.Add(new Coin(new Coordinate2D(171, 98)));
            coins.Add(new Coin(new Coordinate2D(171, 148)));
            coins.Add(new Coin(new Coordinate2D(171, 197)));
            coins.Add(new Coin(new Coordinate2D(171, 246)));

            coins.Add(new Coin(new Coordinate2D(224, 49)));
            coins.Add(new Coin(new Coordinate2D(224, 98)));
            coins.Add(new Coin(new Coordinate2D(224, 148)));
            coins.Add(new Coin(new Coordinate2D(224, 197)));
            coins.Add(new Coin(new Coordinate2D(224, 246)));
            coins.Add(new Coin(new Coordinate2D(224, 295)));
            coins.Add(new Coin(new Coordinate2D(224, 345)));
            coins.Add(new Coin(new Coordinate2D(224, 394)));

            coins.Add(new Coin(new Coordinate2D(277, 49)));
            coins.Add(new Coin(new Coordinate2D(277, 98)));
            coins.Add(new Coin(new Coordinate2D(277, 148)));
            coins.Add(new Coin(new Coordinate2D(277, 197)));
            coins.Add(new Coin(new Coordinate2D(277, 246)));
            coins.Add(new Coin(new Coordinate2D(277, 295)));
            coins.Add(new Coin(new Coordinate2D(277, 345)));
            coins.Add(new Coin(new Coordinate2D(277, 394)));

            coins.Add(new Coin(new Coordinate2D(331, 197)));
            coins.Add(new Coin(new Coordinate2D(331, 246)));
            coins.Add(new Coin(new Coordinate2D(331, 295)));
            coins.Add(new Coin(new Coordinate2D(331, 345)));
            coins.Add(new Coin(new Coordinate2D(331, 394)));

            coins.Add(new Coin(new Coordinate2D(384, 49)));
            coins.Add(new Coin(new Coordinate2D(384, 98)));
            coins.Add(new Coin(new Coordinate2D(384, 148)));
            coins.Add(new Coin(new Coordinate2D(384, 197)));
            coins.Add(new Coin(new Coordinate2D(384, 246)));

            coins.Add(new Coin(new Coordinate2D(437, 49)));
            coins.Add(new Coin(new Coordinate2D(437, 98)));
            coins.Add(new Coin(new Coordinate2D(437, 148)));
            coins.Add(new Coin(new Coordinate2D(437, 197)));
            coins.Add(new Coin(new Coordinate2D(437, 246)));
            coins.Add(new Coin(new Coordinate2D(437, 295)));
            coins.Add(new Coin(new Coordinate2D(437, 345)));
            coins.Add(new Coin(new Coordinate2D(437, 394)));

            return coins;
        }
    }
}
