using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Windows.Forms;
using OGPLib;
using pacman;
using System.IO;

namespace Client {
    class Client {
        private struct RemoteClientURIParams {
            public string protocol;
            public string host;
            public int port;
            public string service_name;
        }
        private static Form1 form;
        private static int lastState = -1;

        private static Pacman pacmanGame; //main game object
        private static RemoteClientURIParams uri;

        public static Form1 Form { get => form; }
        public static int LastState { get => lastState; set => lastState = value; }
        public static Pacman PacmanGame { get => pacmanGame; set => pacmanGame = value; }
        public static bool BotMode { get => botMode; set => botMode = value; }
        public static Bot GameBot { get => gameBot; set => gameBot = value; }
        public static string Filename { get => filename; set => filename = value; }

        public static bool botMode = false; //is reading input from file?
        private static Bot gameBot = null;
        private static string filename = null;

        static void Main(string[] args) { //TODO args: client_url, filename (optional); command parser
            Client.parseArgs(args);
            Console.WriteLine(buildURIFromParams(uri));

            Thread gui_thread = new Thread(startGUI);
            gui_thread.Start();


            //Connect to main server and replicas and get games
            Client.PacmanGame = new Pacman("user1", uri.host, uri.port, uri.service_name);
            if (!botMode) {
                setupListeners();
            }
        }

        [STAThread]
        static void startGUI() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Client.form = new Form1();
            Application.Run(Client.Form);
        }

        private static void parseArgs(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("Usage: Client.exe client_uri [filename]");
                Console.ReadLine();
                Environment.Exit(-1);
            }

            try {
                Client.Filename = args[1];
                Client.botMode = true;
            } catch (IndexOutOfRangeException) {
                Client.botMode = false;
            }

            Client.uri = parseURIArgs(args[0]);
        }

        /// <summary>
        /// Creates and returns data structure to encapsulate URI fields
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static RemoteClientURIParams parseURIArgs(string args) {
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

        /// <summary>
        /// Creates an URI string from URI params structure.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static string buildURIFromParams(RemoteClientURIParams parameters) {
            return String.Format("{0}://{1}:{2}/{3}",
                parameters.protocol, 
                parameters.host,
                parameters.port,
                parameters.service_name);
        }

        /// <summary>
        /// Initializes event listeners needed for game to run
        /// </summary>
        private static void setupListeners() {
            Client.form.KeyDown += new KeyEventHandler(onGameboardKeydown);
            Client.form.Controls["tbMsg"].KeyDown += new KeyEventHandler(onChatBoxKeydown);
        }

        private static void onGameboardKeydown(object sender, KeyEventArgs ev) {
            // we don't want to send Enter keys to server
            if (ev.KeyCode == Keys.Enter) {
                TextBox msgbox = form.Controls["tbMsg"] as TextBox;
                Utils.doInGUI(Client.form, delegate () {
                    msgbox.Enabled = true;
                    form.Controls["tbMsg"].Focus();
                });
            } else {

                //Submit Input to main server
                try
                {
                    Client.PacmanGame.Game.sumbitInput(new GameInput(ev.KeyValue), Client.PacmanGame.Player1);
                }

                catch(Exception e)
                {
                    //TIMEOUT?
                    IRemoteGame newPrimary = Client.PacmanGame.GameReplicas[0];
                    Client.PacmanGame.Game = newPrimary;
                    Client.PacmanGame.Game.SetAsPrimary();
                    Client.PacmanGame.GameReplicas.RemoveAt(0);

                    //repeat the command
                    Client.PacmanGame.Game.sumbitInput(new GameInput(ev.KeyValue), Client.PacmanGame.Player1);
                }


                //Submit Input to replicas 
                foreach (var r in Client.PacmanGame.GameReplicas)
                {
                    r.sumbitInput(new GameInput(ev.KeyValue), Client.PacmanGame.Player1);
                }
            }
        }

        private static void onChatBoxKeydown(object sender, KeyEventArgs ev) {
            TextBox tbox = sender as TextBox;
            if (ev.KeyCode == Keys.Enter) {
                Client.PacmanGame.Chat.broadcast(tbox.Text);
                Utils.doInGUI(Client.form, delegate () {
                    tbox.Text = "";
                    tbox.Enabled = false;
                    Client.form.Focus();
                });
            }
        }
    }

    public class Pacman {
        private List<IRemoteGame> gameReplicas;
        private IRemoteGame game;

        private Chat chat;
        private List<OGPPlayer> players;
        private OGPPlayer player1;
        private GameParameters parameters;
        private RemoteClient clientServices;
        private PacmanState currentState;

        public Pacman(string username, string client_ip, int client_port, string client_name) {

            String serversFilePath = "..\\..\\..\\PuppetMaster\\AdditionalFiles\\servers.txt";

            //String mainserver_uri = "tcp://localhost:20001/Server";
            //List<string> replica_uris = new List<string>(new string[] { "tcp://localhost:20002/Server"});
            List<string> replica_uris = new List<string>(new string[] {});

            StreamReader file = new StreamReader(serversFilePath);
            String mainserver_uri = file.ReadLine();
            String line;

            while ((line = file.ReadLine()) != null)
            {
                replica_uris.Add(line);
            }

            file.Close();

            TcpChannel channelServer = new TcpChannel(client_port);
            ChannelServices.RegisterChannel(channelServer, false);

            //find main game
            this.Game = (Activator.GetObject(typeof(IRemoteGame), mainserver_uri) as IRemoteGame);

            String test = this.Game.ping();

            //find replicas
            this.gameReplicas = new List<IRemoteGame>();
            foreach(var replica_uri in replica_uris)
            {
                this.gameReplicas.Add((Activator.GetObject(typeof(IRemoteGame), replica_uri) as IRemoteGame));
            }

            //Create and expose client services            
            this.ClientServices = new RemoteClient();
            RemotingServices.Marshal(ClientServices, client_name, typeof(RemoteClient));

            string client_uri_game = string.Format("tcp://{0}:{1}/{2}", client_ip, client_port, client_name);
            string client_uri_chat = string.Format("tcp://{0}:{1}/{2}", client_ip, client_port, Chat.CHAT_REMOTE_NAME);

            this.player1 = new OGPPlayer(username, client_uri_game, client_uri_chat);

            //register on replicas
            foreach (var g in this.gameReplicas)
            {
                g.register(this.player1, false);
                String test2 = this.Game.ping();
            }

            //register on main game
            this.Parameters = this.Game.register(this.player1, true);


            this.Player1.ChatProcessName = this.Parameters.chatIndex;
            
        }

        public void initChat() {
            this.chat = new Chat(this);
            this.chat.MessageReceivedEvent += messageReceivedHandler;
            this.chat.enable();
        }

        public void messageReceivedHandler(object sender, MessageReceivedEventArgs args) {
            string message = String.Format("<{0}>: {1}\r\n", args.Sender, args.Message);
            Utils.doInGUI(Client.Form, delegate () {
                Client.Form.Controls["tbChat"].Text += message;
            });
        }
        
        public Chat Chat { get => chat; set => chat = value; }
        public GameParameters Parameters { get => parameters; set => parameters = value; }
        public List<OGPPlayer> Players { get => players; set => players = value; }
        public OGPPlayer Player1 { get => player1; set => player1 = value; }
        public RemoteClient ClientServices { get => clientServices; set => clientServices = value; }
        public PacmanState CurrentState { get => currentState; set => currentState = value; }
        public List<IRemoteGame> GameReplicas { get => gameReplicas; set => gameReplicas = value; }
        public IRemoteGame Game { get => game; set => game = value; }
    }


    public class RemoteClient : MarshalByRefObject, IRemoteClient {
        public event EventHandler<StateReceivedEventArgs> StateReceivedEvent;
        private int delay = 0;

        public void SetDelay(int d)
        {
            this.delay = d;
        }

        public void begin(GameState initialState) {
            Task.Delay(delay).Wait();

            Client.PacmanGame.CurrentState = initialState as PacmanState;
            List<OGPPlayer> players = new List<OGPPlayer>();
            foreach (var p in Client.PacmanGame.CurrentState.Players) {
                players.Add(p.Value.Player1);
            }
            Client.PacmanGame.CurrentState.draw(Client.Form);
            Client.PacmanGame.Players = players;
            Client.PacmanGame.initChat();
            if (Client.botMode) {
                Client.GameBot = new Bot(Client.Filename, Client.PacmanGame);
                Client.GameBot.execute();
            }
        }

        public void deliverState(GameState state) {
            Task.Delay(delay).Wait();

            if (state.RoundId > Client.LastState) {
                DeltaPacmanState delta = state as DeltaPacmanState;
                Client.PacmanGame.CurrentState += delta;
                Client.Form.applyDelta(delta);
                Client.LastState = state.RoundId;
                StateReceivedEvent?.Invoke(this, new StateReceivedEventArgs(state));
            }
        }

        public string LocalState(string pid, int round_id)
        {
            Task.Delay(delay).Wait();

            int localRoundID = Client.PacmanGame.CurrentState.RoundId;
            String result = "PID -> " + pid + "LocalRoundID -> " + localRoundID + "\n";
            String aux;

            foreach (var p in Client.PacmanGame.CurrentState.Players)
            {
                aux = p.Value.IsDead ? "L" : "P";
                result += "Pacman id -> " + p.Value.Guid + " Playing/Lost? -> " + aux + ", Coords -> " + p.Value.Coordinates +"\n";
            }

            foreach (var m in Client.PacmanGame.CurrentState.Monsters)
            {
                result += "Monster id -> " + m.Value.Guid + ", Coords -> " +m.Value.Coordinates + "\n";
            }

            foreach (var c in Client.PacmanGame.CurrentState.Coins)
            {
                result += "Coin id -> " + c.Value.Guid + ", Coords -> " + c.Value.Coordinates + "\n";
            }

            foreach (var w in Client.PacmanGame.CurrentState.Walls)
            {
                result += "Wall id -> " + w.Value.Guid + ", Coords -> " + w.Value.Coordinates + "\n";
            }


            return result;
        }

        public string ping()
        {
            return "pong";
        }
    }

    public class StateReceivedEventArgs : EventArgs {
        private GameState state;

        public StateReceivedEventArgs(GameState state) {
            this.state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public GameState State { get => state; set => state = value; }
    }
}
