using OGPLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace PuppetMasterLib
{
    public class PCS : MarshalByRefObject
    {
        private Dictionary<String, Process> pid_process_dict= new Dictionary<string, Process>();
        private Dictionary<String, IRemoteEntity> pid_instance_dict= new Dictionary<string, IRemoteEntity>();

        public string Hello()
        {
            return "hello";
        }

        public string Status(string pid)
        {
            //TODO
            return "Status from pid -> " + pid;
        }

        public string Crash(string pid)
        {
            Process p = null;
            pid_process_dict.TryGetValue(pid, out p);

            if (p != null)
            {
                p.CloseMainWindow();

                return "Crashing pid -> " + pid;
            }

            return "Crashing pid failed -> unknown pid " + pid;
        }

        public string Freeze(string pid)
        {
            return "Freezing pid -> " + pid;
        }

        public string Unfreeze(string pid)
        {
            return "Freezing pid -> " + pid;
        }

        public string LocalState(string pid, int round_id)
        {
            Console.WriteLine("Localstate on pid -> " + pid);
            String localstate = "localstate not found";
            IRemoteEntity e = null;
            pid_instance_dict.TryGetValue(pid, out e);
            if (e != null)
            {
               localstate = e.LocalState(pid, round_id);
            }
            Console.WriteLine("Localstate on pid -> " + localstate);

            return localstate;
        }

        public void StartServer(string pid, string server_url, int msec_per_round, int num_players)
        {
            Console.WriteLine("Server started on " + server_url);
            //string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"\Server\bin\Debug\Server.exe");

            string path = "..\\..\\..\\Server\\bin\\Debug\\Server.exe";
            Console.WriteLine(path);

            //"Usage: Server.exe server_uri number_of_players round_time(ms)"
            string args = server_url + " " + num_players + " " + msec_per_round;
            Process p = StartProgram(path,args);
            pid_process_dict.Add(pid, p);

            //IRemoteEntity server = Activator.GetObject(typeof(IRemoteGame), server_url) as IRemoteEntity;
            IRemoteGame server = Activator.GetObject(typeof(IRemoteGame), server_url) as IRemoteGame;

            pid_instance_dict.Add(pid, server);

            String swag = server.LocalState("2",1);
            String style = "222";
        }

        public void StartClient(string pid, string client_url, int msec_per_round, int num_players)
        {
            Console.WriteLine("Client started on " + client_url);
            string path = "..\\..\\..\\Client\\bin\\Debug\\Client.exe";
            Console.WriteLine(path);
            string args = client_url;
            Process p = StartProgram(path, args);
            pid_process_dict.Add(pid, p);

            IRemoteEntity client = Activator.GetObject(typeof(IRemoteClient), client_url) as IRemoteEntity;
            pid_instance_dict.Add(pid, client);
        }

        private Process StartProgram(string path, string args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            Console.WriteLine(Directory.GetCurrentDirectory());
            startInfo.FileName = path;
            startInfo.Arguments = args;
            return Process.Start(startInfo);
        }
    }
}
