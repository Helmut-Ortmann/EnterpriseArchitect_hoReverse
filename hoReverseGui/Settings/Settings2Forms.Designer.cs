namespace hoReverse.Settings2
{
    partial class Settings2Forms
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
            this.cmbLogicalType = new System.Windows.Forms.ComboBox();
            this.cmbLogicalStereotype = new System.Windows.Forms.ComboBox();
            this.grdLogical = new System.Windows.Forms.DataGridView();
            this.grdLogicalConnectorType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdLogicalStereotype = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdLogicalLineStyle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdLogicalIsDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.btnLogicalAdd = new System.Windows.Forms.Button();
            this.btnLogicalRemove = new System.Windows.Forms.Button();
            this.chkBoxLogicalIsDefault = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnActivityRemove = new System.Windows.Forms.Button();
            this.btnActivityAdd = new System.Windows.Forms.Button();
            this.grdActivity = new System.Windows.Forms.DataGridView();
            this.grdActivityConnectorType = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdActivityStereotype = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdActivityLineStyle = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.grdActivityIsDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.chkBoxActivityIsDefault = new System.Windows.Forms.CheckBox();
            this.cmbActivityStereotype = new System.Windows.Forms.ComboBox();
            this.cmbActivityType = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cmbLogicalLineStyle = new System.Windows.Forms.ComboBox();
            this.cmbActivityLineStyle = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.grdLogical)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdActivity)).BeginInit();
            this.SuspendLayout();
            // 
            // cmbLogicalType
            // 
            this.cmbLogicalType.FormattingEnabled = true;
            this.cmbLogicalType.Location = new System.Drawing.Point(23, 33);
            this.cmbLogicalType.Name = "cmbLogicalType";
            this.cmbLogicalType.Size = new System.Drawing.Size(121, 21);
            this.cmbLogicalType.TabIndex = 0;
            // 
            // cmbLogicalStereotype
            // 
            this.cmbLogicalStereotype.FormattingEnabled = true;
            this.cmbLogicalStereotype.Location = new System.Drawing.Point(150, 33);
            this.cmbLogicalStereotype.Name = "cmbLogicalStereotype";
            this.cmbLogicalStereotype.Size = new System.Drawing.Size(121, 21);
            this.cmbLogicalStereotype.TabIndex = 1;
            // 
            // grdLogical
            // 
            this.grdLogical.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdLogical.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.grdLogicalConnectorType,
            this.grdLogicalStereotype,
            this.grdLogicalLineStyle,
            this.grdLogicalIsDefault});
            this.grdLogical.Location = new System.Drawing.Point(553, 32);
            this.grdLogical.Name = "grdLogical";
            this.grdLogical.Size = new System.Drawing.Size(366, 117);
            this.grdLogical.TabIndex = 3;
            // 
            // grdLogicalConnectorType
            // 
            this.grdLogicalConnectorType.HeaderText = "ConnectorType";
            this.grdLogicalConnectorType.Name = "grdLogicalConnectorType";
            this.grdLogicalConnectorType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // grdLogicalStereotype
            // 
            this.grdLogicalStereotype.HeaderText = "Stereotype";
            this.grdLogicalStereotype.Name = "grdLogicalStereotype";
            this.grdLogicalStereotype.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // grdLogicalLineStyle
            // 
            this.grdLogicalLineStyle.HeaderText = "LineStyle";
            this.grdLogicalLineStyle.Name = "grdLogicalLineStyle";
            // 
            // grdLogicalIsDefault
            // 
            this.grdLogicalIsDefault.HeaderText = "Default";
            this.grdLogicalIsDefault.Name = "grdLogicalIsDefault";
            this.grdLogicalIsDefault.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.grdLogicalIsDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // btnLogicalAdd
            // 
            this.btnLogicalAdd.Location = new System.Drawing.Point(472, 32);
            this.btnLogicalAdd.Name = "btnLogicalAdd";
            this.btnLogicalAdd.Size = new System.Drawing.Size(75, 23);
            this.btnLogicalAdd.TabIndex = 4;
            this.btnLogicalAdd.Text = "==>";
            this.btnLogicalAdd.UseVisualStyleBackColor = true;
            this.btnLogicalAdd.Click += new System.EventHandler(this.btnLogicalAdd_Click);
            // 
            // btnLogicalRemove
            // 
            this.btnLogicalRemove.Location = new System.Drawing.Point(472, 61);
            this.btnLogicalRemove.Name = "btnLogicalRemove";
            this.btnLogicalRemove.Size = new System.Drawing.Size(75, 23);
            this.btnLogicalRemove.TabIndex = 5;
            this.btnLogicalRemove.Text = "<==";
            this.btnLogicalRemove.UseVisualStyleBackColor = true;
            this.btnLogicalRemove.Click += new System.EventHandler(this.btnLogicalRemove_Click);
            // 
            // chkBoxLogicalIsDefault
            // 
            this.chkBoxLogicalIsDefault.AutoSize = true;
            this.chkBoxLogicalIsDefault.Location = new System.Drawing.Point(295, 33);
            this.chkBoxLogicalIsDefault.Name = "chkBoxLogicalIsDefault";
            this.chkBoxLogicalIsDefault.Size = new System.Drawing.Size(67, 17);
            this.chkBoxLogicalIsDefault.TabIndex = 2;
            this.chkBoxLogicalIsDefault.Text = "isDefault";
            this.chkBoxLogicalIsDefault.UseVisualStyleBackColor = true;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(617, 594);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(712, 594);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnActivityRemove
            // 
            this.btnActivityRemove.Location = new System.Drawing.Point(472, 251);
            this.btnActivityRemove.Name = "btnActivityRemove";
            this.btnActivityRemove.Size = new System.Drawing.Size(75, 23);
            this.btnActivityRemove.TabIndex = 13;
            this.btnActivityRemove.Text = "<==";
            this.btnActivityRemove.UseVisualStyleBackColor = true;
            this.btnActivityRemove.Click += new System.EventHandler(this.btnActivityRemove_Click);
            // 
            // btnActivityAdd
            // 
            this.btnActivityAdd.Location = new System.Drawing.Point(472, 222);
            this.btnActivityAdd.Name = "btnActivityAdd";
            this.btnActivityAdd.Size = new System.Drawing.Size(75, 23);
            this.btnActivityAdd.TabIndex = 12;
            this.btnActivityAdd.Text = "==>";
            this.btnActivityAdd.UseVisualStyleBackColor = true;
            this.btnActivityAdd.Click += new System.EventHandler(this.btnActivityAdd_Click);
            // 
            // grdActivity
            // 
            this.grdActivity.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdActivity.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.grdActivityConnectorType,
            this.grdActivityStereotype,
            this.grdActivityLineStyle,
            this.grdActivityIsDefault});
            this.grdActivity.Location = new System.Drawing.Point(553, 222);
            this.grdActivity.Name = "grdActivity";
            this.grdActivity.Size = new System.Drawing.Size(366, 117);
            this.grdActivity.TabIndex = 11;
            // 
            // grdActivityConnectorType
            // 
            this.grdActivityConnectorType.HeaderText = "ConnectorType";
            this.grdActivityConnectorType.Name = "grdActivityConnectorType";
            this.grdActivityConnectorType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // grdActivityStereotype
            // 
            this.grdActivityStereotype.HeaderText = "Stereotype";
            this.grdActivityStereotype.Name = "grdActivityStereotype";
            this.grdActivityStereotype.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // grdActivityLineStyle
            // 
            this.grdActivityLineStyle.HeaderText = "Linestyle";
            this.grdActivityLineStyle.Name = "grdActivityLineStyle";
            this.grdActivityLineStyle.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.grdActivityLineStyle.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // grdActivityIsDefault
            // 
            this.grdActivityIsDefault.HeaderText = "Default";
            this.grdActivityIsDefault.Name = "grdActivityIsDefault";
            this.grdActivityIsDefault.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.grdActivityIsDefault.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            // 
            // chkBoxActivityIsDefault
            // 
            this.chkBoxActivityIsDefault.AutoSize = true;
            this.chkBoxActivityIsDefault.Location = new System.Drawing.Point(295, 222);
            this.chkBoxActivityIsDefault.Name = "chkBoxActivityIsDefault";
            this.chkBoxActivityIsDefault.Size = new System.Drawing.Size(67, 17);
            this.chkBoxActivityIsDefault.TabIndex = 10;
            this.chkBoxActivityIsDefault.Text = "isDefault";
            this.chkBoxActivityIsDefault.UseVisualStyleBackColor = true;
            // 
            // cmbActivityStereotype
            // 
            this.cmbActivityStereotype.FormattingEnabled = true;
            this.cmbActivityStereotype.Location = new System.Drawing.Point(150, 222);
            this.cmbActivityStereotype.Name = "cmbActivityStereotype";
            this.cmbActivityStereotype.Size = new System.Drawing.Size(121, 21);
            this.cmbActivityStereotype.TabIndex = 9;
            // 
            // cmbActivityType
            // 
            this.cmbActivityType.FormattingEnabled = true;
            this.cmbActivityType.Location = new System.Drawing.Point(23, 222);
            this.cmbActivityType.Name = "cmbActivityType";
            this.cmbActivityType.Size = new System.Drawing.Size(121, 21);
            this.cmbActivityType.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(98, 13);
            this.label1.TabIndex = 14;
            this.label1.Text = "Logical Diagram";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(20, 194);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Activity Diagram";
            // 
            // cmbLogicalLineStyle
            // 
            this.cmbLogicalLineStyle.FormattingEnabled = true;
            this.cmbLogicalLineStyle.Location = new System.Drawing.Point(369, 33);
            this.cmbLogicalLineStyle.Name = "cmbLogicalLineStyle";
            this.cmbLogicalLineStyle.Size = new System.Drawing.Size(85, 21);
            this.cmbLogicalLineStyle.TabIndex = 16;
            // 
            // cmbActivityLineStyle
            // 
            this.cmbActivityLineStyle.FormattingEnabled = true;
            this.cmbActivityLineStyle.Location = new System.Drawing.Point(369, 224);
            this.cmbActivityLineStyle.Name = "cmbActivityLineStyle";
            this.cmbActivityLineStyle.Size = new System.Drawing.Size(85, 21);
            this.cmbActivityLineStyle.TabIndex = 17;
            // 
            // Settings2Forms
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1042, 649);
            this.Controls.Add(this.cmbActivityLineStyle);
            this.Controls.Add(this.cmbLogicalLineStyle);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnActivityRemove);
            this.Controls.Add(this.btnActivityAdd);
            this.Controls.Add(this.grdActivity);
            this.Controls.Add(this.chkBoxActivityIsDefault);
            this.Controls.Add(this.cmbActivityStereotype);
            this.Controls.Add(this.cmbActivityType);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnLogicalRemove);
            this.Controls.Add(this.btnLogicalAdd);
            this.Controls.Add(this.grdLogical);
            this.Controls.Add(this.chkBoxLogicalIsDefault);
            this.Controls.Add(this.cmbLogicalStereotype);
            this.Controls.Add(this.cmbLogicalType);
            this.Name = "Settings2Forms";
            this.Text = "Settings2Forms";
            this.Shown += new System.EventHandler(this.Settings2Forms_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.grdLogical)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.grdActivity)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox cmbLogicalType;
        private System.Windows.Forms.ComboBox cmbLogicalStereotype;
        private System.Windows.Forms.DataGridView grdLogical;
        private System.Windows.Forms.Button btnLogicalAdd;
        private System.Windows.Forms.Button btnLogicalRemove;
        private System.Windows.Forms.CheckBox chkBoxLogicalIsDefault;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnActivityRemove;
        private System.Windows.Forms.Button btnActivityAdd;
        private System.Windows.Forms.DataGridView grdActivity;
        private System.Windows.Forms.CheckBox chkBoxActivityIsDefault;
        private System.Windows.Forms.ComboBox cmbActivityStereotype;
        private System.Windows.Forms.ComboBox cmbActivityType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridViewTextBoxColumn grdLogicalConnectorType;
        private System.Windows.Forms.DataGridViewTextBoxColumn grdLogicalStereotype;
        private System.Windows.Forms.DataGridViewTextBoxColumn grdLogicalLineStyle;
        private System.Windows.Forms.DataGridViewCheckBoxColumn grdLogicalIsDefault;
        private System.Windows.Forms.DataGridViewTextBoxColumn grdActivityConnectorType;
        private System.Windows.Forms.DataGridViewTextBoxColumn grdActivityStereotype;
        private System.Windows.Forms.DataGridViewTextBoxColumn grdActivityLineStyle;
        private System.Windows.Forms.DataGridViewCheckBoxColumn grdActivityIsDefault;
        private System.Windows.Forms.ComboBox cmbLogicalLineStyle;
        private System.Windows.Forms.ComboBox cmbActivityLineStyle;
    }
}