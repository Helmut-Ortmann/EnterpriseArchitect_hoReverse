namespace hoReverse.Services.AutoCpp.Analyze
{
    partial class FrmFunctions
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmFunctions));
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkOnlyImplementations = new System.Windows.Forms.CheckBox();
            this.chkOnlyMacros = new System.Windows.Forms.CheckBox();
            this.txtFilterFile = new System.Windows.Forms.TextBox();
            this.txtFilterImplementation = new System.Windows.Forms.TextBox();
            this.txtFilterFunction = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txtVcSymbolDb = new System.Windows.Forms.TextBox();
            this.txtSourceFolder = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.menuStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.dataGridView1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(929, 693);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(929, 20);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            this.toolTip1.SetToolTip(this.menuStrip1, "Grid of the inventorized *.c and header files:\r\n- Implementations from *.c or *.c" +
        "pp\r\n- Macros from *.h or *.hpp");
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(3, 103);
            this.dataGridView1.MultiSelect = false;
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(923, 587);
            this.dataGridView1.TabIndex = 1;
            this.toolTip1.SetToolTip(this.dataGridView1, "Grid of the inventorized *.c and header files:\r\n- Implementations from *.c or *.c" +
        "pp\r\n- Macros from *.h or *.hpp");
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.chkOnlyImplementations);
            this.panel1.Controls.Add(this.chkOnlyMacros);
            this.panel1.Controls.Add(this.txtFilterFile);
            this.panel1.Controls.Add(this.txtFilterImplementation);
            this.panel1.Controls.Add(this.txtFilterFunction);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtVcSymbolDb);
            this.panel1.Controls.Add(this.txtSourceFolder);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(923, 74);
            this.panel1.TabIndex = 2;
            this.toolTip1.SetToolTip(this.panel1, "Grid of the inventorized *.c and header files:\r\n- Implementations from *.c or *.c" +
        "pp\r\n- Macros from *.h or *.hpp");
            // 
            // chkOnlyImplementations
            // 
            this.chkOnlyImplementations.AutoSize = true;
            this.chkOnlyImplementations.Location = new System.Drawing.Point(705, 49);
            this.chkOnlyImplementations.Name = "chkOnlyImplementations";
            this.chkOnlyImplementations.Size = new System.Drawing.Size(143, 17);
            this.chkOnlyImplementations.TabIndex = 3;
            this.chkOnlyImplementations.Text = "Implented  by C-Function";
            this.toolTip1.SetToolTip(this.chkOnlyImplementations, "If checked only Implementations are shown");
            this.chkOnlyImplementations.UseVisualStyleBackColor = true;
            this.chkOnlyImplementations.CheckedChanged += new System.EventHandler(this.chkOnlyMacros_CheckedChanged);
            // 
            // chkOnlyMacros
            // 
            this.chkOnlyMacros.AutoSize = true;
            this.chkOnlyMacros.Location = new System.Drawing.Point(854, 49);
            this.chkOnlyMacros.Name = "chkOnlyMacros";
            this.chkOnlyMacros.Size = new System.Drawing.Size(60, 17);
            this.chkOnlyMacros.TabIndex = 3;
            this.chkOnlyMacros.Text = "macros";
            this.toolTip1.SetToolTip(this.chkOnlyMacros, "If checked only Macros are shown");
            this.chkOnlyMacros.UseVisualStyleBackColor = true;
            this.chkOnlyMacros.CheckedChanged += new System.EventHandler(this.chkOnlyMacros_CheckedChanged);
            // 
            // txtFilterFile
            // 
            this.txtFilterFile.Location = new System.Drawing.Point(557, 49);
            this.txtFilterFile.Name = "txtFilterFile";
            this.txtFilterFile.Size = new System.Drawing.Size(129, 20);
            this.txtFilterFile.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txtFilterFile, resources.GetString("txtFilterFile.ToolTip"));
            this.txtFilterFile.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilterFunction_KeyPress);
            // 
            // txtFilterImplementation
            // 
            this.txtFilterImplementation.Location = new System.Drawing.Point(414, 49);
            this.txtFilterImplementation.Name = "txtFilterImplementation";
            this.txtFilterImplementation.Size = new System.Drawing.Size(129, 20);
            this.txtFilterImplementation.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txtFilterImplementation, resources.GetString("txtFilterImplementation.ToolTip"));
            this.txtFilterImplementation.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilterFunction_KeyPress);
            // 
            // txtFilterFunction
            // 
            this.txtFilterFunction.Location = new System.Drawing.Point(0, 54);
            this.txtFilterFunction.Name = "txtFilterFunction";
            this.txtFilterFunction.Size = new System.Drawing.Size(129, 20);
            this.txtFilterFunction.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txtFilterFunction, resources.GetString("txtFilterFunction.ToolTip"));
            this.txtFilterFunction.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilterFunction_KeyPress);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(46, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Symbols";
            this.toolTip1.SetToolTip(this.label2, "Path of the SQLite Symbol VC Code db.");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Source";
            // 
            // txtVcSymbolDb
            // 
            this.txtVcSymbolDb.Location = new System.Drawing.Point(51, 28);
            this.txtVcSymbolDb.Name = "txtVcSymbolDb";
            this.txtVcSymbolDb.Size = new System.Drawing.Size(846, 20);
            this.txtVcSymbolDb.TabIndex = 0;
            this.toolTip1.SetToolTip(this.txtVcSymbolDb, resources.GetString("txtVcSymbolDb.ToolTip"));
            // 
            // txtSourceFolder
            // 
            this.txtSourceFolder.Location = new System.Drawing.Point(51, 7);
            this.txtSourceFolder.Name = "txtSourceFolder";
            this.txtSourceFolder.Size = new System.Drawing.Size(846, 20);
            this.txtSourceFolder.TabIndex = 0;
            this.toolTip1.SetToolTip(this.txtSourceFolder, "The C source folder where the *.c/*.cpp and *.h/*.hpp files are.\r\n\r\nYou have to s" +
        "et it in Settings.\r\n\r\nTo make sure it works:\r\nShow at least one C/C++ file with " +
        "VC Code. \r\n\r\n");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(135, 55);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(273, 15);
            this.label3.TabIndex = 10;
            this.label3.Text = "Filter are \' AND \', \'*\' at start for arbitrary beginning";
            // 
            // FrmFunctions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 693);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FrmFunctions";
            this.Text = "FrmFunctions";
            this.toolTip1.SetToolTip(this, "Grid of the inventorized *.c and header files:\r\n- Implementations from *.c or *.c" +
        "pp\r\n- Macros from *.h or *.hpp");
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSourceFolder;
        private System.Windows.Forms.TextBox txtFilterFunction;
        private System.Windows.Forms.CheckBox chkOnlyMacros;
        private System.Windows.Forms.TextBox txtFilterFile;
        private System.Windows.Forms.TextBox txtFilterImplementation;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkOnlyImplementations;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtVcSymbolDb;
        private System.Windows.Forms.Label label3;
    }
}