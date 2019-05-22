using PCS_service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetMaster
    {

        private List<PCS_connection> PCS_connections = new List<PCS_connection>();
        private string plot_path;
        private Dictionary<string, PCS> pid_pcs_Dict = new Dictionary<string, PCS>();

        PuppetMaster(string path)
        {
            plot_path = path;
        }

        static void Main(string[] args)
        {
            string path = null;

            if (args.Length == 0)
            {
                System.Console.WriteLine("Please enter the plot file path.");
                path = Console.ReadLine();
                return;
            }
            else
            {
                path = args[0];
            }

            PuppetMaster p = new PuppetMaster(path);
            p.Idle();

        }

        private void Idle()
        {

            if (plot_path != null)
            {
                ParsePlotFile(plot_path);
            }

            while (true)
            {
                var input = Console.ReadLine();

                switch (input)
                {
                    case "e":
                        return;

                    case "h":
                        System.Console.WriteLine("Available commands: ");
                        System.Console.WriteLine("  Add_PCS <URL>");
                        System.Console.WriteLine("  GlobalStatus");
                        System.Console.WriteLine("  Crash <PID>");
                        System.Console.WriteLine("  Freeze <PID>");
                        System.Console.WriteLine("  Unfreeze <PID>");
                        System.Console.WriteLine("  InjectDelay <src_PID> <dst_PID>");
                        System.Console.WriteLine("  LocalState <PID>");
                        System.Console.WriteLine("  Script <NAME>");

                        break;

                    default:
                        ParseCommand(input);
                        break;
                }

            }
        }

        private void ParseCommand(string line)
        {
            string[] splitString = line.Split();
            string command = splitString[0];
            string src_PID, dst_PID;
            string pid;
            int round_id;
            string pcs_url;
            string expose_url;
            int msec_per_round;
            int num_players;

            Thread t;

            switch (command)
            {
                case "StartServer":
                    //server unique id
                    pid = splitString[1];

                    //process creation service url
                    pcs_url = splitString[2];

                    //url where client/server exposes its services 
                    expose_url = splitString[3];

                    //round timeout
                    msec_per_round = Int32.Parse(splitString[4]);
                    num_players = Int32.Parse(splitString[5]);

                    StartServer(pid, pcs_url, expose_url, msec_per_round, num_players);
                    break;

                case "StartClient":
                    //client unique id
                    pid = splitString[1];

                    //process creation service url
                    pcs_url = splitString[2];

                    //url where client/server exposes its services 
                    expose_url = splitString[3];

                    //round timeout
                    msec_per_round = Int32.Parse(splitString[4]);
                    num_players = Int32.Parse(splitString[5]);

                    StartClient(pid, pcs_url, expose_url, msec_per_round, num_players);
                    break;

                case "GlobalStatus":
                    //Thread t1 = new Thread(() => GlobalStatus());
                    //t1.Start();
                    GlobalStatus();

                    break;

                case "Crash":
                    pid = splitString[1];
                    //t = new Thread(() => Crash(pid));
                    //t.Start();
                    Crash(pid);
                    break;

                case "Freeze":
                    pid = splitString[1];
                    //t = new Thread(() => Freeze(pid));
                    //t.Start();
                    Freeze(pid);
                    break;

                case "Unfreeze":
                    pid = splitString[1];
                    //t = new Thread(() => Unfreeze(pid));
                    //t.Start();
                    Unfreeze(pid);
                    break;

                case "InjectDelay":
                    src_PID = splitString[1];
                    dst_PID = splitString[2];

                    InjectDelay(src_PID, dst_PID);
                    //t = new Thread(() => InjectDelay());
                    //t.Start();

                    break;

                case "LocalState":
                    pid = splitString[1];
                    round_id = Int32.Parse(splitString[2]);
                    LocalState(pid, round_id);
                    //t = new Thread(() => LocalState(pid, round_id));
                    //t.Start();

                    break;

                case "Wait":
                    int delay = Int32.Parse(splitString[1]);
                    Thread.Sleep(delay);

                    break;
            }
        }

        private void ParsePlotFile(string plot_path)
        {
            string line;

            // Read the file and display it line by line.  
            StreamReader file = new StreamReader(plot_path);

            while ((line = file.ReadLine()) != null)
            {
                ParseCommand(line);
            }

            file.Close();
            System.Console.WriteLine("Plot file parsing completed, awaiting commands (h->help, e->exit): ");
        }

        private void GlobalStatus()
        {
            /* GlobalStatus: This command makes all processes in the system print their current
            status. The status command should present brief information about the state of the
            system (who is present, which nodes are presumed failed, etc...). Status information
            can be printed on each nodes’ and does not need to be centralized at the PuppetMaster.*/

            //TODO (who is present, which nodes are presumed failed, etc...)
            string pid;
            PCS pcs;
            string pidNames = "";

            Console.WriteLine("PuppetMaster GlobalStatus");
            Console.WriteLine(" # of PCS Connections -> " + this.PCS_connections.Count);
            

            foreach (KeyValuePair<string, PCS> entry in pid_pcs_Dict)
            {
                pid = entry.Key;
                pidNames += pid + ", ";
                pcs = (PCS)entry.Value;
                Console.WriteLine(pcs.Status(pid));
            }

            Console.WriteLine(" Active Processes -> " + pidNames);
            Console.WriteLine("-----------------------");
        }

        private void Crash(string pid)
        {
            PCS pcs = null;
            pid_pcs_Dict.TryGetValue(pid, out pcs);
            if (pcs != null)
            {
                Console.WriteLine(pcs.Crash(pid));
            }
            //process crashed -> remove pid from list
            pid_pcs_Dict.Remove(pid);
        }

        private void Freeze(string pid)
        {
            PCS pcs = null;
            pid_pcs_Dict.TryGetValue(pid, out pcs);
            if (pcs != null)
            {
                Console.WriteLine(pcs.Freeze(pid));
            }

        }

        private void Unfreeze(string pid)
        {
            PCS pcs = null;
            pid_pcs_Dict.TryGetValue(pid, out pcs);
            if (pcs != null)
            {
                Console.WriteLine(pcs.Unfreeze(pid));
            }

        }

        private void InjectDelay(string pid_src, string pid_dst)
        {
            PCS pcs = null;
            pid_pcs_Dict.TryGetValue(pid_src, out pcs);
            if (pcs != null)
            {
                Console.WriteLine(pcs.InjectDelay(pid_src, pid_dst));
            }
        }

        private void LocalState(string pid, int round_id)
        {
            string gameState = "";
            PCS pcs = null;

            pid_pcs_Dict.TryGetValue(pid, out pcs);
            if (pcs != null)
            {
                gameState = pcs.LocalState(pid, round_id);
            }

            String filename = "LocalState-" + pid + "-" + round_id + ".txt";
            File.WriteAllText(@"..\\..\\AdditionalFiles\\" + filename, gameState);
        }

        private PCS AddPCS(string url)
        {
            PCS_connection pcs = GetPCS_connection(url);

            if (pcs == null)
            {
                Console.WriteLine("Adding new PCS connection");
                pcs = new PCS_connection(url);
                PCS_connections.Add(pcs);
            }
            else
            {
                Console.WriteLine("PCS connection already exists, not adding");
            }
            return pcs.pcs;
        }

        private PCS_connection GetPCS_connection(string url)
        {
            foreach (PCS_connection pcs in PCS_connections)
            {
                if (pcs.url.Equals(url))
                {
                    return pcs;
                }
            }
            Console.WriteLine("PCS connection not found");
            return null;
        }

        private void StartServer(string pid, string pcs_url, string server_url, int msec_per_round, int num_players)
        {
            System.Console.WriteLine("Starting server pid -> " + pid);
            PCS pcs = AddPCS(pcs_url);
            pid_pcs_Dict.Add(pid, pcs);

            pcs.StartServer(pid, server_url, msec_per_round, num_players);
        }

        private void StartClient(string pid, string pcs_url, string client_url, int msec_per_round, int num_players)
        {
            System.Console.WriteLine("Starting client pid -> " + pid);
            PCS pcs = AddPCS(pcs_url);
            pid_pcs_Dict.Add(pid, pcs);

            pcs.StartClient(pid, client_url, msec_per_round, num_players);
        }
    }
}
