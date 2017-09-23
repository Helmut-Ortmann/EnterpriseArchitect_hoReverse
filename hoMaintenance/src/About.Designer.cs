namespace hoMaintenance
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tDescriptionCompleteness1 = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.tCreated = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.txtPath = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tDescriptionCompleteness1
            // 
            this.tDescriptionCompleteness1.BackColor = System.Drawing.SystemColors.Control;
            this.tDescriptionCompleteness1.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tDescriptionCompleteness1.Location = new System.Drawing.Point(18, 43);
            this.tDescriptionCompleteness1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tDescriptionCompleteness1.Multiline = true;
            this.tDescriptionCompleteness1.Name = "tDescriptionCompleteness1";
            this.tDescriptionCompleteness1.ReadOnly = true;
            this.tDescriptionCompleteness1.Size = new System.Drawing.Size(453, 486);
            this.tDescriptionCompleteness1.TabIndex = 0;
            this.tDescriptionCompleteness1.Text = "Copy Select statement to Clipboard\r\n\r\nChange Name to Synonym\r\n\r\nMKS Get Newest (C" +
    "heckout, Undo checkout)\r\n\r\nGet VC latest member (Member Revision)\r\n\r\n";
            this.tDescriptionCompleteness1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(317, 654);
            this.linkLabel1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(269, 20);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "mailto:Helmut.Ortmann@T-Online.de";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // tCreated
            // 
            this.tCreated.Location = new System.Drawing.Point(13, 539);
            this.tCreated.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.tCreated.Multiline = true;
            this.tCreated.Name = "tCreated";
            this.tCreated.ReadOnly = true;
            this.tCreated.Size = new System.Drawing.Size(296, 135);
            this.tCreated.TabIndex = 3;
            this.tCreated.Text = "Helmut Ortmann\r\n- Germany\r\n- Untere Sonnenhalde 3\r\n- 88636 Illmensee\r\n\r\n(+49) 172" +
    " / 51 79 167\r\n";
            this.tCreated.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(18, 4);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(781, 29);
            this.textBox1.TabIndex = 4;
            this.textBox1.Text = "Helper for EA maintenance purposes.";
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged_1);
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.Location = new System.Drawing.Point(317, 595);
            this.lblVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblVersion.MinimumSize = new System.Drawing.Size(160, 0);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(160, 20);
            this.lblVersion.TabIndex = 5;
            this.lblVersion.Text = "Version: xx.xx.xx.xx";
            this.lblVersion.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox2.Location = new System.Drawing.Point(522, 43);
            this.textBox2.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(430, 486);
            this.textBox2.TabIndex = 6;
            this.textBox2.TextChanged += new System.EventHandler(this.textBox2_TextChanged_1);
            // 
            // txtPath
            // 
            this.txtPath.Location = new System.Drawing.Point(321, 539);
            this.txtPath.MinimumSize = new System.Drawing.Size(400, 4);
            this.txtPath.Name = "txtPath";
            this.txtPath.ReadOnly = true;
            this.txtPath.Size = new System.Drawing.Size(631, 26);
            this.txtPath.TabIndex = 7;
            // 
            // About
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(965, 688);
            this.Controls.Add(this.txtPath);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.tCreated);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.tDescriptionCompleteness1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.Name = "About";
            this.Text = " About & Help";
            this.Load += new System.EventHandler(this.About_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tDescriptionCompleteness1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.TextBox tCreated;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.TextBox txtPath;
    }
}