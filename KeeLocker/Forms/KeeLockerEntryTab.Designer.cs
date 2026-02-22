
namespace KeeLocker.Forms
{
	partial class KeeLockerEntryTab
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
      this.chk_UnlockOnOpening = new System.Windows.Forms.CheckBox();
      this.chk_UnlockOnConnection = new System.Windows.Forms.CheckBox();
      this.btn_Unlock = new System.Windows.Forms.Button();
      this.grp_Drive = new System.Windows.Forms.GroupBox();
      this.btn_AddMachine = new System.Windows.Forms.Button();
      this.btn_Delete = new System.Windows.Forms.Button();
      this.btn_Add = new System.Windows.Forms.Button();
      this.lvMounts = new System.Windows.Forms.ListView();
      this.colGUID = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colMount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colComputerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colMachineId = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
      this.tx_Custom = new System.Windows.Forms.TextBox();
      this.btn_RefreshVolumes = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.cbx_SystemVolume = new KeeLocker.Forms.RichComboBox();
      this.btn_Clear = new System.Windows.Forms.Button();
      this.grp_Unlock = new System.Windows.Forms.GroupBox();
      this.icon = new System.Windows.Forms.PictureBox();
      this.chk_IsRecoveryKey = new System.Windows.Forms.CheckBox();
      this.txt_Info = new System.Windows.Forms.Label();
      this.grp_Drive.SuspendLayout();
      this.grp_Unlock.SuspendLayout();
      ((System.ComponentModel.ISupportInitialize)(this.icon)).BeginInit();
      this.SuspendLayout();
      // 
      // chk_UnlockOnOpening
      // 
      this.chk_UnlockOnOpening.AutoSize = true;
      this.chk_UnlockOnOpening.Location = new System.Drawing.Point(35, 34);
      this.chk_UnlockOnOpening.Margin = new System.Windows.Forms.Padding(4);
      this.chk_UnlockOnOpening.Name = "chk_UnlockOnOpening";
      this.chk_UnlockOnOpening.Size = new System.Drawing.Size(249, 20);
      this.chk_UnlockOnOpening.TabIndex = 106;
      this.chk_UnlockOnOpening.Text = "Unlock volume on database opening";
      this.chk_UnlockOnOpening.UseVisualStyleBackColor = true;
      this.chk_UnlockOnOpening.Click += new System.EventHandler(this.chk_UnlockOnOpening_Click);
      // 
      // chk_UnlockOnConnection
      // 
      this.chk_UnlockOnConnection.AutoSize = true;
      this.chk_UnlockOnConnection.Location = new System.Drawing.Point(35, 70);
      this.chk_UnlockOnConnection.Margin = new System.Windows.Forms.Padding(4);
      this.chk_UnlockOnConnection.Name = "chk_UnlockOnConnection";
      this.chk_UnlockOnConnection.Size = new System.Drawing.Size(218, 20);
      this.chk_UnlockOnConnection.TabIndex = 107;
      this.chk_UnlockOnConnection.Text = "Unlock volume when connected";
      this.chk_UnlockOnConnection.UseVisualStyleBackColor = true;
      this.chk_UnlockOnConnection.Click += new System.EventHandler(this.chk_UnlockOnConnection_Click);
      // 
      // btn_Unlock
      // 
      this.btn_Unlock.Location = new System.Drawing.Point(332, 27);
      this.btn_Unlock.Margin = new System.Windows.Forms.Padding(4);
      this.btn_Unlock.Name = "btn_Unlock";
      this.btn_Unlock.Size = new System.Drawing.Size(149, 28);
      this.btn_Unlock.TabIndex = 109;
      this.btn_Unlock.Text = "Unlock Volume Now";
      this.btn_Unlock.UseVisualStyleBackColor = true;
      this.btn_Unlock.Click += new System.EventHandler(this.btn_Unlock_Click);
      // 
      // grp_Drive
      // 
      this.grp_Drive.Controls.Add(this.btn_AddMachine);
      this.grp_Drive.Controls.Add(this.btn_Delete);
      this.grp_Drive.Controls.Add(this.btn_Add);
      this.grp_Drive.Controls.Add(this.lvMounts);
      this.grp_Drive.Controls.Add(this.tx_Custom);
      this.grp_Drive.Controls.Add(this.btn_RefreshVolumes);
      this.grp_Drive.Controls.Add(this.label1);
      this.grp_Drive.Controls.Add(this.cbx_SystemVolume);
      this.grp_Drive.Location = new System.Drawing.Point(12, 9);
      this.grp_Drive.Margin = new System.Windows.Forms.Padding(4);
      this.grp_Drive.Name = "grp_Drive";
      this.grp_Drive.Padding = new System.Windows.Forms.Padding(4);
      this.grp_Drive.Size = new System.Drawing.Size(586, 225);
      this.grp_Drive.TabIndex = 112;
      this.grp_Drive.TabStop = false;
      this.grp_Drive.Text = "Drive info (On different computers a drive may have different ids)";
      // 
      // btn_AddMachine
      // 
      this.btn_AddMachine.Location = new System.Drawing.Point(475, 162);
      this.btn_AddMachine.Margin = new System.Windows.Forms.Padding(4);
      this.btn_AddMachine.Name = "btn_AddMachine";
      this.btn_AddMachine.Size = new System.Drawing.Size(105, 28);
      this.btn_AddMachine.TabIndex = 116;
      this.btn_AddMachine.Text = "Add (this PC)";
      this.btn_AddMachine.UseVisualStyleBackColor = true;
      this.btn_AddMachine.Click += new System.EventHandler(this.btn_AddMachine_Click);
      // 
      // btn_Delete
      // 
      this.btn_Delete.Location = new System.Drawing.Point(278, 162);
      this.btn_Delete.Margin = new System.Windows.Forms.Padding(4);
      this.btn_Delete.Name = "btn_Delete";
      this.btn_Delete.Size = new System.Drawing.Size(76, 28);
      this.btn_Delete.TabIndex = 115;
      this.btn_Delete.Text = "Delete";
      this.btn_Delete.UseVisualStyleBackColor = true;
      this.btn_Delete.Click += new System.EventHandler(this.btn_Delete_Click);
      // 
      // btn_Add
      // 
      this.btn_Add.Location = new System.Drawing.Point(376, 162);
      this.btn_Add.Margin = new System.Windows.Forms.Padding(4);
      this.btn_Add.Name = "btn_Add";
      this.btn_Add.Size = new System.Drawing.Size(105, 28);
      this.btn_Add.TabIndex = 111;
      this.btn_Add.Text = "Add (any PC)";
      this.btn_Add.UseVisualStyleBackColor = true;
      this.btn_Add.Click += new System.EventHandler(this.btn_Add_Click);
      // 
      // lvMounts
      // 
      this.lvMounts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colGUID,
            this.colMount,
            this.colComputerName,
            this.colMachineId,
            this.colType});
      this.lvMounts.FullRowSelect = true;
      this.lvMounts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
      this.lvMounts.HideSelection = false;
      this.lvMounts.Location = new System.Drawing.Point(13, 22);
      this.lvMounts.MultiSelect = false;
      this.lvMounts.Name = "lvMounts";
      this.lvMounts.Size = new System.Drawing.Size(567, 133);
      this.lvMounts.TabIndex = 114;
      this.lvMounts.UseCompatibleStateImageBehavior = false;
      this.lvMounts.View = System.Windows.Forms.View.Details;
      this.lvMounts.SelectedIndexChanged += new System.EventHandler(this.lvMounts_SelectedIndexChanged);
      // 
      // colGUID
      // 
      this.colGUID.Text = "Volume";
      this.colGUID.Width = 184;
      // 
      // colMount
      // 
      this.colMount.Text = "Mount Point";
      this.colMount.Width = 111;
      // 
      // colComputerName
      // 
      this.colComputerName.Text = "ComputerName";
      this.colComputerName.Width = 88;
      // 
      // colMachineId
      // 
      this.colMachineId.Text = "MachineId";
      this.colMachineId.Width = 110;
      // 
      // colType
      // 
      this.colType.Text = "Type";
      this.colType.Width = 64;
      // 
      // tx_Custom
      // 
      this.tx_Custom.Location = new System.Drawing.Point(259, 193);
      this.tx_Custom.Margin = new System.Windows.Forms.Padding(2);
      this.tx_Custom.Name = "tx_Custom";
      this.tx_Custom.Size = new System.Drawing.Size(321, 22);
      this.tx_Custom.TabIndex = 113;
      this.tx_Custom.TextChanged += new System.EventHandler(this.tx_Custom_TextChanged);
      this.tx_Custom.Enter += new System.EventHandler(this.tx_Custom_Enter);
      this.tx_Custom.Leave += new System.EventHandler(this.tx_Custom_Leave);
      this.tx_Custom.Validated += new System.EventHandler(this.tx_Custom_Validated);
      // 
      // btn_RefreshVolumes
      // 
      this.btn_RefreshVolumes.Location = new System.Drawing.Point(120, 162);
      this.btn_RefreshVolumes.Margin = new System.Windows.Forms.Padding(4);
      this.btn_RefreshVolumes.Name = "btn_RefreshVolumes";
      this.btn_RefreshVolumes.Size = new System.Drawing.Size(133, 28);
      this.btn_RefreshVolumes.TabIndex = 111;
      this.btn_RefreshVolumes.Text = "Refresh Volumes";
      this.btn_RefreshVolumes.UseVisualStyleBackColor = true;
      this.btn_RefreshVolumes.Click += new System.EventHandler(this.btn_RefreshVolumes_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(13, 168);
      this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(95, 16);
      this.label1.TabIndex = 107;
      this.label1.Text = "Select volume:";
      // 
      // cbx_SystemVolume
      // 
      this.cbx_SystemVolume.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
      this.cbx_SystemVolume.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbx_SystemVolume.Location = new System.Drawing.Point(13, 193);
      this.cbx_SystemVolume.Margin = new System.Windows.Forms.Padding(4);
      this.cbx_SystemVolume.Name = "cbx_SystemVolume";
      this.cbx_SystemVolume.SelectedData = null;
      this.cbx_SystemVolume.Size = new System.Drawing.Size(240, 23);
      this.cbx_SystemVolume.TabIndex = 106;
      this.cbx_SystemVolume.SelectedIndexChanged += new System.EventHandler(this.cbx_SystemVolume_SelectedIndexChanged);
      // 
      // btn_Clear
      // 
      this.btn_Clear.Location = new System.Drawing.Point(427, 63);
      this.btn_Clear.Margin = new System.Windows.Forms.Padding(4);
      this.btn_Clear.Name = "btn_Clear";
      this.btn_Clear.Size = new System.Drawing.Size(149, 28);
      this.btn_Clear.TabIndex = 110;
      this.btn_Clear.Text = "Clear KeeLocker";
      this.btn_Clear.UseVisualStyleBackColor = true;
      this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
      // 
      // grp_Unlock
      // 
      this.grp_Unlock.Controls.Add(this.icon);
      this.grp_Unlock.Controls.Add(this.chk_IsRecoveryKey);
      this.grp_Unlock.Controls.Add(this.chk_UnlockOnOpening);
      this.grp_Unlock.Controls.Add(this.btn_Clear);
      this.grp_Unlock.Controls.Add(this.chk_UnlockOnConnection);
      this.grp_Unlock.Controls.Add(this.btn_Unlock);
      this.grp_Unlock.Location = new System.Drawing.Point(12, 242);
      this.grp_Unlock.Margin = new System.Windows.Forms.Padding(4);
      this.grp_Unlock.Name = "grp_Unlock";
      this.grp_Unlock.Padding = new System.Windows.Forms.Padding(4);
      this.grp_Unlock.Size = new System.Drawing.Size(588, 135);
      this.grp_Unlock.TabIndex = 111;
      this.grp_Unlock.TabStop = false;
      this.grp_Unlock.Text = "Unlock settings";
      // 
      // icon
      // 
      this.icon.InitialImage = null;
      this.icon.Location = new System.Drawing.Point(559, 107);
      this.icon.Margin = new System.Windows.Forms.Padding(2);
      this.icon.Name = "icon";
      this.icon.Size = new System.Drawing.Size(18, 18);
      this.icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.icon.TabIndex = 110;
      this.icon.TabStop = false;
      this.icon.Click += new System.EventHandler(this.icon_Click);
      // 
      // chk_IsRecoveryKey
      // 
      this.chk_IsRecoveryKey.AutoSize = true;
      this.chk_IsRecoveryKey.Location = new System.Drawing.Point(35, 107);
      this.chk_IsRecoveryKey.Margin = new System.Windows.Forms.Padding(4);
      this.chk_IsRecoveryKey.Name = "chk_IsRecoveryKey";
      this.chk_IsRecoveryKey.Size = new System.Drawing.Size(232, 20);
      this.chk_IsRecoveryKey.TabIndex = 108;
      this.chk_IsRecoveryKey.Text = "Password field is the recovery key";
      this.chk_IsRecoveryKey.UseVisualStyleBackColor = true;
      this.chk_IsRecoveryKey.Click += new System.EventHandler(this.chk_IsRecoveryKey_Click);
      // 
      // txt_Info
      // 
      this.txt_Info.AutoSize = true;
      this.txt_Info.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.txt_Info.Location = new System.Drawing.Point(0, 383);
      this.txt_Info.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
      this.txt_Info.Name = "txt_Info";
      this.txt_Info.Size = new System.Drawing.Size(42, 16);
      this.txt_Info.TabIndex = 108;
      this.txt_Info.Text = "status";
      // 
      // KeeLockerEntryTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.txt_Info);
      this.Controls.Add(this.grp_Unlock);
      this.Controls.Add(this.grp_Drive);
      this.Margin = new System.Windows.Forms.Padding(4);
      this.Name = "KeeLockerEntryTab";
      this.Size = new System.Drawing.Size(617, 399);
      this.grp_Drive.ResumeLayout(false);
      this.grp_Drive.PerformLayout();
      this.grp_Unlock.ResumeLayout(false);
      this.grp_Unlock.PerformLayout();
      ((System.ComponentModel.ISupportInitialize)(this.icon)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.CheckBox chk_UnlockOnOpening;
		private System.Windows.Forms.CheckBox chk_UnlockOnConnection;
		private System.Windows.Forms.Button btn_Unlock;
		private System.Windows.Forms.GroupBox grp_Drive;
		private System.Windows.Forms.GroupBox grp_Unlock;
		private System.Windows.Forms.CheckBox chk_IsRecoveryKey;
		private System.Windows.Forms.Label label1;
		private RichComboBox cbx_SystemVolume;
		private System.Windows.Forms.Label txt_Info;
		private System.Windows.Forms.Button btn_Clear;
		private System.Windows.Forms.PictureBox icon;
		private System.Windows.Forms.Button btn_RefreshVolumes;
		private System.Windows.Forms.TextBox tx_Custom;
		private System.Windows.Forms.ListView lvMounts;
		private System.Windows.Forms.ColumnHeader colGUID;
		private System.Windows.Forms.ColumnHeader colMount;
		private System.Windows.Forms.ColumnHeader colComputerName;
		private System.Windows.Forms.ColumnHeader colType;
		private System.Windows.Forms.ColumnHeader colMachineId;
		private System.Windows.Forms.Button btn_Delete;
		private System.Windows.Forms.Button btn_Add;
		private System.Windows.Forms.Button btn_AddMachine;
	}
}