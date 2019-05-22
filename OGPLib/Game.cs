using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGPLib {
    /// <summary>
    /// Encapsulates user input for server to process
    /// </summary>
    [Serializable]
    public class GameInput {
        int key;

        public GameInput(int key) {
            this.Key = key;
        }

        public int Key { get => key; set => key = value; }
    }

    [Serializable]
    public class Coordinate2D {
        private int x;
        private int y;

        public Coordinate2D(int x, int y) {
            this.x = x;
            this.y = y;
        }

        public int X { get => x; set => x = value; }
        public int Y { get => y; set => y = value; }

        public override string ToString() { return "( " + x + "," + y + " )"; }
    }


    [Serializable]
    public abstract class GameState {
        private bool gameover = false;
        private int round_id;

        public GameState(int roundId, bool gameover = false) {
            this.Gameover = gameover;
            this.RoundId = roundId;
        }

        public bool Gameover { get => gameover; set => gameover = value; }
        public int RoundId { get => round_id; set => round_id = value; }
    }

    [Serializable]
    public class OGPPlayer {
        private string username;
        private string client_game_Uri;
        private string chatUri;
        private string chatProcessName;  // represents the 'index' in vector clock

        public OGPPlayer(string username, string client_game_uri, string chatUri) {
            this.username = username ?? "unknown user";
            this.chatUri = chatUri ?? throw new ArgumentNullException(nameof(chatUri));
            this.client_game_Uri = client_game_uri ?? throw new ArgumentNullException(nameof(client_game_Uri));
        }

        public OGPPlayer(string username, string client_game_uri, string chatUri, string chatProcessName) : this(username, client_game_uri, chatUri) {
            this.chatProcessName = chatProcessName ?? throw new ArgumentNullException(nameof(chatProcessName));
        }

        public string Username { get => this.username; set => username = value; }
        public string Client_game_Uri { get => this.client_game_Uri; set => this.client_game_Uri = value; }
        public string ChatUri { get => this.chatUri; set => this.chatUri = value; }
        public string ChatProcessName { get => chatProcessName; set => chatProcessName = value; }
        public string PlayerID { get => ChatProcessName; }

        public override bool Equals(object obj) {
            OGPPlayer player = obj as OGPPlayer;
            return player != null && (this.PlayerID == player.PlayerID);
        }

        public override int GetHashCode() { //Needed to be able to index from a dictionary
            return PlayerID.GetHashCode();
        }
    }

    [Serializable]
    public class GameParameters {
        public string chatIndex;
        public int numPlayers;
        public int MSEC_PER_ROUND;

        public GameParameters(string chatIndex, int numPlayers, int MSEC_PER_ROUND) {
            this.chatIndex = chatIndex;
            this.numPlayers = numPlayers;
            this.MSEC_PER_ROUND = MSEC_PER_ROUND;
        }
    }
}
