using OGPLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace pacman {
    [Serializable]
    public class PacmanState : GameState {
        private Dictionary<string, Monster> monsters = new Dictionary<string, Monster>();
        private Dictionary<string, Coin> coins = new Dictionary<string, Coin>();
        private Dictionary<string, Player> players = new Dictionary<string, Player>();
        private Dictionary<string, Wall> walls = new Dictionary<string, Wall>();

        [NonSerialized]
        private BoardSize boardSize = new BoardSize(xMax: 320, yMax: 300, xMin: 0, yMin: 40);

        public Dictionary<string, Monster> Monsters { get => monsters; set => monsters = value; }
        public Dictionary<string, Coin> Coins { get => coins; set => coins = value; }
        public Dictionary<string, Player> Players { get => players; set => players = value; }
        public Dictionary<string, Wall> Walls { get => walls; set => walls = value; }

        public PacmanState(int roundId, bool gameover = false) : base(roundId, gameover) {
            this.monsters = new Dictionary<string, Monster>();
            this.coins = new Dictionary<string, Coin>();
            this.players = new Dictionary<string, Player>();
        }

        public PacmanState(IEnumerable<Monster> monsters1,
                           IEnumerable<Coin> coins1,
                           IEnumerable<Player> dplayers,
                           IEnumerable<Wall> walls1,
                           int roundId, bool gameOver = false) : base(roundId, gameOver) {
            foreach (var m in monsters1) {
                this.Monsters.Add(m.Guid, m);
            }
            foreach (var c in coins1) {
                this.Coins.Add(c.Guid, c);
            }
            foreach (var p in dplayers) {
                this.Players.Add(p.Guid, p);
            }
            foreach (var w in walls1) {
                this.Walls.Add(w.Guid, w);
            }
        }

        public void draw(Form form) {
            Form1 f = form as Form1;

            f.clean();

            //TODO better way to do this?
            foreach (var m in Monsters) {
                m.Value.draw(f);
            }
            foreach (var c in Coins) {
                c.Value.draw(f);
            }
            foreach (var p in Players) {
                p.Value.draw(f);
            }
            foreach (var w in walls) {
                w.Value.draw(f);
            }

        }

        public DeltaPacmanState RefreshGame(Dictionary<OGPPlayer, GameInput> player_inputs) {
            DeltaPacmanState delta = null;
            if (!this.Gameover) {
                delta = new DeltaPacmanState(++this.RoundId);
                foreach (var p in players) {
                    if (!p.Value.IsDead) {
                        OGPPlayer ogpplayer = p.Value.Player1;
                        GameInput input = null;

                        if (player_inputs.ContainsKey(ogpplayer)) {
                            input = player_inputs[ogpplayer];
                        }
                        Direction direction = ExtractDirection(input);
                        bool moved = p.Value.Move(direction, 5, boardSize);
                        if (moved) {
                            delta.updatedPlayers.Add(p.Value);
                        }
                    }
                }

                CheckCoinColision(delta); //checks for coin collision
                CheckDeathColision(delta); //checks for deaths
                UpdateMonstersState(delta); //updates red and yellow positions
                if (checkGameOver()) delta.Gameover = true;
            }
            return delta;
        }

        private bool checkGameOver() {
            //TODO
            //if there is at leat one coin "alive", the game is not over
            bool ret = this.Coins.Count == 0;
            int dead_players = 0;

            if (!ret) {
                foreach (var p in Players) {
                    
                    //if there is at leat one player alive, the game is not over
                    if (!p.Value.IsDead) return false;
                    else { ++dead_players; }
                }
            }
            return ret || dead_players==Players.Count();
        }
        // Check for player collisions
        public void CheckCoinColision(DeltaPacmanState delta) {
        List<Coin> coins2remove = new List<Coin>();
            foreach (var p in Players) {
                if (!p.Value.IsDead) {
                    foreach (var c in Coins) {
                        if (p.Value.Model.Bounds.IntersectsWith(c.Value.Model.Bounds)) {
                            p.Value.Score++;
                            coins2remove.Add(c.Value);
                            delta.eatenCoins.Add(c.Value);
                        }
                    }
                }
            }
            foreach (Coin c in coins2remove) {
                Coins.Remove(c.Guid);
            }
        }

        public void CheckDeathColision(DeltaPacmanState delta) {
            foreach (var p in Players) {
                foreach (var m in Monsters) {
                    if (p.Value.Model.Bounds.IntersectsWith(m.Value.Model.Bounds)) {
                        p.Value.IsDead = true;
                        delta.updatedPlayers.Add(p.Value);
                        break;
                    }
                }
                foreach (var w in Walls) {
                    if (p.Value.Model.Bounds.IntersectsWith(w.Value.Model.Bounds)) {
                        p.Value.IsDead = true;
                        delta.updatedPlayers.Add(p.Value);
                        break;
                    }
                }
            }
        }


        // Update monsters position
        public void UpdateMonstersState(DeltaPacmanState delta) {
            foreach (var monster in Monsters) {
                Monster m = monster.Value;
                m.Move(m.Direction, 5, boardSize);
                Rectangle mBounds = m.Model.Bounds;
                foreach (var w in Walls) {
                    Rectangle wBounds = w.Value.Model.Bounds;
                    if (mBounds.IntersectsWith(wBounds)) {
                        m.InvertDirectionX();
                        break;
                    }
                }

                if (m.Left(boardSize) < boardSize.xMin || m.Right(boardSize) > boardSize.xMax) {
                    m.InvertDirectionX();
                }

                if (m.Top(boardSize) < boardSize.yMin || m.Bottom(boardSize) > boardSize.yMax - 2) {
                    m.InvertDirectionY();
                }
                delta.updatedMonsters.Add(m);
            }
        }


        private Direction ExtractDirection(GameInput gameInput) {
            if (gameInput == null) { return Direction.NONE; }
            Keys k = (Keys) gameInput.Key;
            switch (k) {
                case (Keys.Up):
                    return Direction.UP;
                case (Keys.Down):
                    return Direction.DOWN;
                case (Keys.Left):
                    return Direction.LEFT;
                case (Keys.Right):
                    return Direction.RIGHT;
                default:
                    return Direction.NONE;
            }
        }

        public static PacmanState operator +(PacmanState state, DeltaPacmanState delta) {
            foreach (var coin in delta.eatenCoins) {
                state.Coins.Remove(coin.Guid);
            }
            foreach (var monster in delta.updatedMonsters) {
                state.Monsters[monster.Guid] = monster;
            }
            foreach (var player in delta.updatedPlayers) {
                state.Players[player.Guid] = player;
            }
            return state;
        }
    }

    [Serializable]
    public class DeltaPacmanState : GameState {
        public HashSet<Coin> eatenCoins = new HashSet<Coin>();
        public HashSet<Player> updatedPlayers = new HashSet<Player>();
        public HashSet<Monster> updatedMonsters = new HashSet<Monster>();


        public DeltaPacmanState(int roundId, bool gameover = false) : base(roundId, gameover) {
        }
    }
}
