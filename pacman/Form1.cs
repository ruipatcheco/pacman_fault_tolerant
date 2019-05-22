using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;



namespace pacman {
    public partial class Form1 : Form { 

        public Form1() {
            InitializeComponent();
            label2.Visible = false;
        }

        public void clean() {
            // Removing from iterator you are currently iterating over is a bad idea. We need to copy first
            // and then remove.
            List<Control> controls = new List<Control>();
            foreach (Control c in this.Controls.OfType<PictureBox>()) {
                controls.Add(c);
            }
            Utils.doInGUI(this, delegate () {
                foreach(Control c in controls) {
                    this.Controls.Remove(c);
                }
            });
        }

        public void updateScores(PacmanState state) {
            string scores = "";
            foreach (var player in state.Players) {
                scores += String.Format("{0}: {1}\r\n", player.Value.Player1.Username, player.Value.Score);
            }
            Utils.doInGUI(this, delegate () {
                this.Controls["scoresGroup"].Controls["scores_tb"].Text = scores;
            });
        }

        public void applyDelta(DeltaPacmanState delta) {
            if (delta.Gameover)
            {
                //TODO 
                Console.WriteLine("The game is over");
                return;
            }
            Utils.doInGUI(this, delegate () {
                foreach (var c in delta.eatenCoins) {
                    this.Controls.Remove(this.Controls[c.Guid]);
                }
                foreach (var m in delta.updatedMonsters) {
                    this.Controls[m.Guid].Location = new Point(m.Coordinates.X, m.Coordinates.Y);
                }
                foreach (var p in delta.updatedPlayers) {
                    this.Controls[p.Guid].Location = new Point(p.Coordinates.X, p.Coordinates.Y);
                }
            });
        }
    }
}
