using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Globalization;
using System.Windows.Forms;
using OGPLib;

namespace Client {
    public class Bot {
        private string[] commands;
        private int index = 0;
        private Command currentCommand;
        private Pacman game;
        private bool finished = false;

        private struct Command {
            public int round_id;
            public string command;

            public Command(int round_id, string command) {
                this.round_id = round_id;
                this.command = command ?? throw new ArgumentNullException(nameof(command));
            }
        }

        public Bot(string filename, Pacman game) {
            commands = System.IO.File.ReadAllLines(@filename);
            updateCurrentCommand();
            this.game = game ?? throw new ArgumentNullException(nameof(game));
        }

        public void execute() {
            updateCurrentCommand();
        }

        private Command parseLine(string line) {
            string[] args = line.Split(',');
            return new Command(Int32.Parse(args[0]), args[1]);
        }

        private static int getInputFromCommand(Command command) {
            if (command.command == "LEFT") return (int) Keys.Left;
            if (command.command == "RIGHT") return (int) Keys.Right;
            if (command.command == "UP") return (int) Keys.Up;
            if (command.command == "DOWN") return (int) Keys.Down;
            return (int) Keys.None;
        }

        private void updateCurrentCommand() {
            try {
                string s = commands[index];
                this.currentCommand = parseLine(s);
                index++;
            } catch (IndexOutOfRangeException) {
                this.finished = true;
            }
        }

        private void executeCommand() {
            //Submit Input to primary
            this.game.Game.sumbitInput(new GameInput(getInputFromCommand(this.currentCommand)), this.game.Player1);
            
            //Submit Input to replicas 
            foreach (var r in Client.PacmanGame.GameReplicas)
            {
                r.sumbitInput(new GameInput(getInputFromCommand(this.currentCommand)), this.game.Player1);
            }
            updateCurrentCommand();
        }

        private void StateReceivedEventHandler(object sender, StateReceivedEventArgs args) {
            if (currentCommand.round_id == args.State.RoundId) {
                executeCommand();
            }
        }

        public int ExecutedCommands { get => index; }
        public bool Finished { get => finished; }
    }
}
