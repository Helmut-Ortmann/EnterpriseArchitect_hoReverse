namespace hoReverse.Services.AutoCpp.Analyze
{
    partial class FrmComponentFunctions
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmComponentFunctions));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.filterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtFilterPathCallee = new System.Windows.Forms.TextBox();
            this.chkOnlyCalledInterfaces = new System.Windows.Forms.CheckBox();
            this.chkOnlyMacros = new System.Windows.Forms.CheckBox();
            this.txtVcSymbolDb = new System.Windows.Forms.TextBox();
            this.txtFolderRoot = new System.Windows.Forms.TextBox();
            this.txtGuid = new System.Windows.Forms.TextBox();
            this.txtComponent = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnOk = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPageProvided = new System.Windows.Forms.TabPage();
            this.grdProvidedInterfaces = new System.Windows.Forms.DataGridView();
            this.tabPageRequired = new System.Windows.Forms.TabPage();
            this.grdRequiredInterfaces = new System.Windows.Forms.DataGridView();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.txtFilterPathImpl = new System.Windows.Forms.TextBox();
            this.menuStrip1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPageProvided.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdProvidedInterfaces)).BeginInit();
            this.tabPageRequired.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdRequiredInterfaces)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(970, 20);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 16);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem,
            this.filterToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 16);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.aboutToolStripMenuItem.Text = "About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // filterToolStripMenuItem
            // 
            this.filterToolStripMenuItem.Name = "filterToolStripMenuItem";
            this.filterToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.filterToolStripMenuItem.Text = "Filter";
            this.filterToolStripMenuItem.Click += new System.EventHandler(this.filterToolStripMenuItem_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.menuStrip1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.tabControl1, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 121F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 36F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(970, 431);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.txtFilterPathImpl);
            this.panel1.Controls.Add(this.txtFilterPathCallee);
            this.panel1.Controls.Add(this.chkOnlyCalledInterfaces);
            this.panel1.Controls.Add(this.chkOnlyMacros);
            this.panel1.Controls.Add(this.txtVcSymbolDb);
            this.panel1.Controls.Add(this.txtFolderRoot);
            this.panel1.Controls.Add(this.txtGuid);
            this.panel1.Controls.Add(this.txtComponent);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 23);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(964, 115);
            this.panel1.TabIndex = 1;
            // 
            // txtFilterPathCallee
            // 
            this.txtFilterPathCallee.Location = new System.Drawing.Point(554, 90);
            this.txtFilterPathCallee.Name = "txtFilterPathCallee";
            this.txtFilterPathCallee.Size = new System.Drawing.Size(119, 20);
            this.txtFilterPathCallee.TabIndex = 8;
            this.toolTip1.SetToolTip(this.txtFilterPathCallee, resources.GetString("txtFilterPathCallee.ToolTip"));
            this.txtFilterPathCallee.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilterPathCallee_KeyPress);
            // 
            // chkOnlyCalledInterfaces
            // 
            this.chkOnlyCalledInterfaces.AutoSize = true;
            this.chkOnlyCalledInterfaces.Location = new System.Drawing.Point(7, 36);
            this.chkOnlyCalledInterfaces.Name = "chkOnlyCalledInterfaces";
            this.chkOnlyCalledInterfaces.Size = new System.Drawing.Size(58, 17);
            this.chkOnlyCalledInterfaces.TabIndex = 6;
            this.chkOnlyCalledInterfaces.Text = "Callee ";
            this.toolTip1.SetToolTip(this.chkOnlyCalledInterfaces, "Check if you want to see only functions called from outside the component");
            this.chkOnlyCalledInterfaces.UseVisualStyleBackColor = true;
            this.chkOnlyCalledInterfaces.CheckedChanged += new System.EventHandler(this.chkOnlyCalledInterfaces_CheckedChanged);
            // 
            // chkOnlyMacros
            // 
            this.chkOnlyMacros.AutoSize = true;
            this.chkOnlyMacros.Location = new System.Drawing.Point(9, 64);
            this.chkOnlyMacros.Name = "chkOnlyMacros";
            this.chkOnlyMacros.Size = new System.Drawing.Size(61, 17);
            this.chkOnlyMacros.TabIndex = 7;
            this.chkOnlyMacros.Text = "Macros";
            this.toolTip1.SetToolTip(this.chkOnlyMacros, "If checked only functions redefined by macros are shown.");
            this.chkOnlyMacros.UseVisualStyleBackColor = true;
            this.chkOnlyMacros.CheckedChanged += new System.EventHandler(this.chkOnlyMacros_CheckedChanged);
            // 
            // txtVcSymbolDb
            // 
            this.txtVcSymbolDb.Location = new System.Drawing.Point(84, 64);
            this.txtVcSymbolDb.Name = "txtVcSymbolDb";
            this.txtVcSymbolDb.ReadOnly = true;
            this.txtVcSymbolDb.Size = new System.Drawing.Size(859, 20);
            this.txtVcSymbolDb.TabIndex = 5;
            this.toolTip1.SetToolTip(this.txtVcSymbolDb, "The VC Code Symbol Database");
            // 
            // txtFolderRoot
            // 
            this.txtFolderRoot.Location = new System.Drawing.Point(84, 37);
            this.txtFolderRoot.Name = "txtFolderRoot";
            this.txtFolderRoot.ReadOnly = true;
            this.txtFolderRoot.Size = new System.Drawing.Size(859, 20);
            this.txtFolderRoot.TabIndex = 4;
            this.toolTip1.SetToolTip(this.txtFolderRoot, "The root folder of source code.\r\n\r\nIf you want to change it do it in Settings ");
            // 
            // txtGuid
            // 
            this.txtGuid.Location = new System.Drawing.Point(271, 11);
            this.txtGuid.Name = "txtGuid";
            this.txtGuid.ReadOnly = true;
            this.txtGuid.Size = new System.Drawing.Size(414, 20);
            this.txtGuid.TabIndex = 2;
            this.toolTip1.SetToolTip(this.txtGuid, "The component GUID");
            // 
            // txtComponent
            // 
            this.txtComponent.Location = new System.Drawing.Point(84, 10);
            this.txtComponent.Name = "txtComponent";
            this.txtComponent.ReadOnly = true;
            this.txtComponent.Size = new System.Drawing.Size(167, 20);
            this.txtComponent.TabIndex = 1;
            this.txtComponent.Text = " ";
            this.toolTip1.SetToolTip(this.txtComponent, "The component to show\r\n- Provided Interfaces\r\n- Required Interfaces");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(61, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Component";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.btnOk);
            this.flowLayoutPanel1.Controls.Add(this.btnCancel);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 398);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(964, 30);
            this.flowLayoutPanel1.TabIndex = 3;
            // 
            // btnOk
            // 
            this.btnOk.Location = new System.Drawing.Point(3, 3);
            this.btnOk.Name = "btnOk";
            this.btnOk.Size = new System.Drawing.Size(75, 23);
            this.btnOk.TabIndex = 0;
            this.btnOk.Text = "Ok";
            this.btnOk.UseVisualStyleBackColor = true;
            this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(84, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPageProvided);
            this.tabControl1.Controls.Add(this.tabPageRequired);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(3, 144);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(964, 248);
            this.tabControl1.TabIndex = 4;
            // 
            // tabPageProvided
            // 
            this.tabPageProvided.Controls.Add(this.grdProvidedInterfaces);
            this.tabPageProvided.Location = new System.Drawing.Point(4, 22);
            this.tabPageProvided.Name = "tabPageProvided";
            this.tabPageProvided.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageProvided.Size = new System.Drawing.Size(956, 222);
            this.tabPageProvided.TabIndex = 0;
            this.tabPageProvided.Text = "Provided";
            this.tabPageProvided.UseVisualStyleBackColor = true;
            // 
            // grdProvidedInterfaces
            // 
            this.grdProvidedInterfaces.AllowUserToAddRows = false;
            this.grdProvidedInterfaces.AllowUserToDeleteRows = false;
            this.grdProvidedInterfaces.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdProvidedInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdProvidedInterfaces.Location = new System.Drawing.Point(3, 3);
            this.grdProvidedInterfaces.MultiSelect = false;
            this.grdProvidedInterfaces.Name = "grdProvidedInterfaces";
            this.grdProvidedInterfaces.RowHeadersVisible = false;
            this.grdProvidedInterfaces.Size = new System.Drawing.Size(950, 216);
            this.grdProvidedInterfaces.TabIndex = 2;
            // 
            // tabPageRequired
            // 
            this.tabPageRequired.Controls.Add(this.grdRequiredInterfaces);
            this.tabPageRequired.Location = new System.Drawing.Point(4, 22);
            this.tabPageRequired.Name = "tabPageRequired";
            this.tabPageRequired.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRequired.Size = new System.Drawing.Size(956, 222);
            this.tabPageRequired.TabIndex = 1;
            this.tabPageRequired.Text = "Required";
            this.tabPageRequired.UseVisualStyleBackColor = true;
            // 
            // grdRequiredInterfaces
            // 
            this.grdRequiredInterfaces.AllowUserToAddRows = false;
            this.grdRequiredInterfaces.AllowUserToDeleteRows = false;
            this.grdRequiredInterfaces.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdRequiredInterfaces.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grdRequiredInterfaces.Location = new System.Drawing.Point(3, 3);
            this.grdRequiredInterfaces.Name = "grdRequiredInterfaces";
            this.grdRequiredInterfaces.Size = new System.Drawing.Size(950, 216);
            this.grdRequiredInterfaces.TabIndex = 0;
            // 
            // txtFilterPathImpl
            // 
            this.txtFilterPathImpl.Location = new System.Drawing.Point(415, 90);
            this.txtFilterPathImpl.Name = "txtFilterPathImpl";
            this.txtFilterPathImpl.Size = new System.Drawing.Size(119, 20);
            this.txtFilterPathImpl.TabIndex = 8;
            this.toolTip1.SetToolTip(this.txtFilterPathImpl, resources.GetString("txtFilterPathImpl.ToolTip"));
            this.txtFilterPathImpl.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtFilterPathCallee_KeyPress);
            // 
            // FrmComponentFunctions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(970, 431);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "FrmComponentFunctions";
            this.Text = "Component Viewer";
            this.Load += new System.EventHandler(this.FrmComponentFunctions_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPageProvided.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdProvidedInterfaces)).EndInit();
            this.tabPageRequired.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grdRequiredInterfaces)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView grdProvidedInterfaces;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button btnOk;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.TextBox txtGuid;
        private System.Windows.Forms.TextBox txtComponent;
        private System.Windows.Forms.TextBox txtFolderRoot;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPageProvided;
        private System.Windows.Forms.TabPage tabPageRequired;
        private System.Windows.Forms.DataGridView grdRequiredInterfaces;
        private System.Windows.Forms.TextBox txtVcSymbolDb;
        private System.Windows.Forms.CheckBox chkOnlyCalledInterfaces;
        private System.Windows.Forms.CheckBox chkOnlyMacros;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtFilterPathCallee;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem filterToolStripMenuItem;
        private System.Windows.Forms.TextBox txtFilterPathImpl;
    }
}
