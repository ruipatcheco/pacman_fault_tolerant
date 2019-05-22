namespace pacman {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.tbMsg = new System.Windows.Forms.TextBox();
            this.tbChat = new System.Windows.Forms.TextBox();
            this.scoresGroup = new System.Windows.Forms.GroupBox();
            this.scores_tb = new System.Windows.Forms.TextBox();
            this.scoresGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(4, 4);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(70, 25);
            this.label1.TabIndex = 71;
            this.label1.Text = "label1";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(237, -1);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 39);
            this.label2.TabIndex = 72;
            this.label2.Text = "label2";
            // 
            // tbMsg
            // 
            this.tbMsg.Enabled = false;
            this.tbMsg.Location = new System.Drawing.Point(489, 388);
            this.tbMsg.Margin = new System.Windows.Forms.Padding(4);
            this.tbMsg.Name = "tbMsg";
            this.tbMsg.Size = new System.Drawing.Size(132, 22);
            this.tbMsg.TabIndex = 143;
            // 
            // tbChat
            // 
            this.tbChat.Enabled = false;
            this.tbChat.Location = new System.Drawing.Point(489, 49);
            this.tbChat.Margin = new System.Windows.Forms.Padding(4);
            this.tbChat.Multiline = true;
            this.tbChat.Name = "tbChat";
            this.tbChat.Size = new System.Drawing.Size(132, 313);
            this.tbChat.TabIndex = 144;
            // 
            // scoresGroup
            // 
            this.scoresGroup.Controls.Add(this.scores_tb);
            this.scoresGroup.Location = new System.Drawing.Point(629, 49);
            this.scoresGroup.Name = "scoresGroup";
            this.scoresGroup.Size = new System.Drawing.Size(160, 361);
            this.scoresGroup.TabIndex = 145;
            this.scoresGroup.TabStop = false;
            this.scoresGroup.Text = "Scores";
            // 
            // scores_tb
            // 
            this.scores_tb.Enabled = false;
            this.scores_tb.Location = new System.Drawing.Point(6, 21);
            this.scores_tb.Multiline = true;
            this.scores_tb.Name = "scores_tb";
            this.scores_tb.Size = new System.Drawing.Size(148, 334);
            this.scores_tb.TabIndex = 0;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(801, 423);
            this.Controls.Add(this.scoresGroup);
            this.Controls.Add(this.tbChat);
            this.Controls.Add(this.tbMsg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "DADman";
            this.scoresGroup.ResumeLayout(false);
            this.scoresGroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbMsg;
        private System.Windows.Forms.TextBox tbChat;
        private System.Windows.Forms.GroupBox scoresGroup;
        private System.Windows.Forms.TextBox scores_tb;
    }
}

