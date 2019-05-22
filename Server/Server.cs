using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Timers;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using OGPLib;
using pacman;
using System.Collections;

namespace Server {
	class Server
	{
        private struct RemoteClientURIParams
        {
            public string protocol;
            public string host;
            public int port;
            public string service_name;
        }

        private struct ServerURIParams
        {
            public string protocol;
            public string host;
            public int port;
            public string service_name;
        }

        public static RemoteGame game;
        private static ServerURIParams uri;

		private static Timer timer;
		//private static Dictionary<OGPPlayer,Player> OGPPlayer_Player = new Dictionary<OGPPlayer, Player>();
        private static List<IRemoteClient> playerRemoteClients;

        public static List<IRemoteClient> PlayerRemoteClients { get => playerRemoteClients; set => playerRemoteClients = value; }

        public static bool isPrimary = false;
        public static List<IRemoteGame> replicas = new List<IRemoteGame>();

        static void Main(string[] args)
		{
            //Parse command line parameters
            Console.WriteLine("{0}", args.Length);
            if (args.Length < 2)
            {
             
                Console.WriteLine("Usage: Server.exe server_uri number_of_players round_time(ms)");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            Server.uri = parse_Server_URIArgs(args[0]);

            int number_players = Int32.Parse(args[1]);
            int round_time = Int32.Parse(args[2]);

            // Create and expose services
            IDictionary props = new Hashtable();
            props["timeout"] = 2000;
            props["port"] = Server.uri.port;
            TcpChannel channel = new TcpChannel(props, new BinaryClientFormatterSinkProvider(), new BinaryServerFormatterSinkProvider());

            ChannelServices.RegisterChannel(channel, false);
            Server.game = new RemoteGame(round_time, number_players);

            // Set up game state
            Server.game.State = StateGenerator.GenerateDefaultState();
            RemotingServices.Marshal(Server.game, Server.uri.service_name, typeof(RemoteGame));

            Console.WriteLine("Initialization complete, waiting for connections... isPrimary = " + Server.isPrimary);

            Console.ReadLine();
        }

     

        public static void BeginGame ()
		{
			Server.timer = new Timer(Server.game.Msec_round);
            PlayerRemoteClients = GetRemoteClients();

            //only broadcast if this is the primary server
            if (isPrimary)
            {
                BroadcastGameStart();
            }

            Server.timer.Elapsed += new ElapsedEventHandler(RefreshGame);
            Server.timer.Enabled = true;

            Console.WriteLine("Let the games begin!");
        }


        private static void RefreshGame (object sender, ElapsedEventArgs args)
		{
            lock (typeof(Server)) {
                var delta = Server.game.State.RefreshGame(Server.game.Player_inputs);

                //only broadcast if this is the primary server
                if (isPrimary)
                {
                    BroadcastGameState(delta);
                }

                ClearInputs();
            }
        }

        private static void ClearInputs()
        {
            Server.game.Player_inputs = new Dictionary<OGPPlayer, GameInput>();
        }

        private static void BroadcastGameStart()
        {
            foreach (IRemoteClient c in PlayerRemoteClients)
            {
                c.begin(game.State);
            }
        }

        private static void BroadcastGameState(DeltaPacmanState delta)
        {
            foreach (IRemoteClient c in PlayerRemoteClients)
            {
                c.deliverState(delta);
            }
        }

        private static List<IRemoteClient> GetRemoteClients()
        {
            List<IRemoteClient> playerRemoteClients = new List<IRemoteClient>();

            foreach (OGPPlayer p in Server.game.getPlayerList())
            {
                string client_services_uri = p.Client_game_Uri;
                RemoteClientURIParams client_services_uri_struct = parseURIArgs(client_services_uri);

                IRemoteClient client = Activator.GetObject(typeof(IRemoteClient), client_services_uri) as IRemoteClient;

                playerRemoteClients.Add(client);
            }
            return playerRemoteClients;
        }

        private static ServerURIParams parse_Server_URIArgs(string args)
        {
            ServerURIParams ret = new ServerURIParams();

            string[] protocol_separated_args = args.Split(new[] { "://" }, StringSplitOptions.None);
            ret.protocol = protocol_separated_args[0];

            string[] host_separated_args = protocol_separated_args[1].Split(':');
            ret.host = host_separated_args[0];

            string[] port_separated_args = host_separated_args[1].Split('/');
            ret.port = Int32.Parse(port_separated_args[0]);
            ret.service_name = port_separated_args[1];

            return ret;
        }

        /// Creates and returns data structure to encapsulate URI fields
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static RemoteClientURIParams parseURIArgs(string args)
        {
            RemoteClientURIParams ret = new RemoteClientURIParams();

            string[] protocol_separated_args = args.Split(new[] { "://" }, StringSplitOptions.None);
            ret.protocol = protocol_separated_args[0];

            string[] host_separated_args = protocol_separated_args[1].Split(':');
            ret.host = host_separated_args[0];

            string[] port_separated_args = host_separated_args[1].Split('/');
            ret.port = Int32.Parse(port_separated_args[0]);
            ret.service_name = port_separated_args[1];

            return ret;
        }
    }

    class RemoteGame : MarshalByRefObject, IRemoteGame
    {
        List<OGPPlayer> players = new List<OGPPlayer>();
		Dictionary<OGPPlayer, GameInput> player_inputs = new Dictionary<OGPPlayer, GameInput>();
		private PacmanState state;
		private int players_registered = 0;
		private int msec_round;
		private int number_players;
        private bool acceptingRegistration = true;
        private int delay = 0;

        public void SetDelay(int d)
        {
            this.delay = d;
        }

        public RemoteGame(int msec, int number_players)
		{
			this.Msec_round = msec;
			this.number_players = number_players;
		}

        public List<OGPPlayer> getPlayerList()
        {
            return players;
        }


        public GameParameters register(OGPPlayer player, bool primary)
        {
            Task.Delay(delay).Wait();

            if (this.acceptingRegistration)
            {
                Console.WriteLine("A new player has joined!");
                Console.WriteLine(player.Client_game_Uri + " has chosen me as primary? -> " + primary);
                Server.isPrimary = primary;

                string chatIndex = String.Format("player{0}", players_registered);
                player.ChatProcessName = chatIndex;
                players.Add(player);
                ++players_registered;
                Player p = new Player(player, new Coordinate2D(8, players.Count * 40), Direction.RIGHT, 0);
                this.State.Players.Add(p.Guid, p);

                if (players.Count == number_players)
                {
                    this.acceptingRegistration = false;
                    Task t = Task.Run(delegate() { Server.BeginGame(); });
                }

                return new GameParameters(chatIndex: chatIndex, numPlayers: number_players, MSEC_PER_ROUND: Msec_round);
            }
            else return null;
        }

        public ResponseStatus sumbitInput(GameInput input, OGPPlayer player)
        {
            Task.Delay(delay).Wait();

            player_inputs[player] = input;
			return ResponseStatus.SUCCESS;
        }

        public string LocalState(string pid, int round_id)
        {
            Task.Delay(delay).Wait();

            int localRoundID = state.RoundId;
            String result = "PID -> " + pid + "LocalRoundID -> " + localRoundID + "\n";
            String aux;

            foreach (var p in state.Players)
            {
                aux = p.Value.IsDead ? "L" : "P";
                result += "Pacman id -> " + p.Value.Guid + " Playing/Lost? -> " + aux + ", Coords -> " + p.Value.Coordinates + "\n";
            }

            foreach (var m in state.Monsters)
            {
                result += "Monster id -> " + m.Value.Guid + ", Coords -> " + m.Value.Coordinates + "\n";
            }

            foreach (var c in state.Coins)
            {
                result += "Coin id -> " + c.Value.Guid + ", Coords -> " + c.Value.Coordinates + "\n";
            }

            foreach (var w in state.Walls)
            {
                result += "Wall id -> " + w.Value.Guid + ", Coords -> " + w.Value.Coordinates + "\n";
            }

            return result;
        }

        public string ping()
        {
            Task.Delay(delay).Wait();

            return "pong";
        }

        public void setState(GameState state)
        {
            Task.Delay(delay).Wait();

            this.State = (PacmanState) state;
        }

        public void SetAsPrimary()
        {
            Task.Delay(delay).Wait();

            Server.isPrimary = true;
            Console.WriteLine("look at me im the captain now ");

        }

        public int Players_registered { get => players_registered; set => players_registered = value; }
		public int Msec_round { get => msec_round; set => msec_round = value; }
		public Dictionary<OGPPlayer, GameInput> Player_inputs { get => player_inputs; set => player_inputs = value; }
		public PacmanState State { get => state; set => state = value; }
    }
}
