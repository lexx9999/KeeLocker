
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
      this.tx_Custom = new System.Windows.Forms.TextBox();
      this.lbl_Custom = new System.Windows.Forms.Label();
      this.btn_RefreshVolumes = new System.Windows.Forms.Button();
      this.btn_Clear = new System.Windows.Forms.Button();
      this.label1 = new System.Windows.Forms.Label();
      this.cbx_SystemVolume = new KeeLocker.Forms.RichComboBox();
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
      this.chk_UnlockOnOpening.Location = new System.Drawing.Point(61, 62);
      this.chk_UnlockOnOpening.Margin = new System.Windows.Forms.Padding(7);
      this.chk_UnlockOnOpening.Name = "chk_UnlockOnOpening";
      this.chk_UnlockOnOpening.Size = new System.Drawing.Size(435, 33);
      this.chk_UnlockOnOpening.TabIndex = 106;
      this.chk_UnlockOnOpening.Text = "Unlock volume on database opening";
      this.chk_UnlockOnOpening.UseVisualStyleBackColor = true;
      this.chk_UnlockOnOpening.Click += new System.EventHandler(this.chk_UnlockOnOpening_Click);
      // 
      // chk_UnlockOnConnection
      // 
      this.chk_UnlockOnConnection.AutoSize = true;
      this.chk_UnlockOnConnection.Location = new System.Drawing.Point(61, 127);
      this.chk_UnlockOnConnection.Margin = new System.Windows.Forms.Padding(7);
      this.chk_UnlockOnConnection.Name = "chk_UnlockOnConnection";
      this.chk_UnlockOnConnection.Size = new System.Drawing.Size(385, 33);
      this.chk_UnlockOnConnection.TabIndex = 107;
      this.chk_UnlockOnConnection.Text = "Unlock volume when connected";
      this.chk_UnlockOnConnection.UseVisualStyleBackColor = true;
      this.chk_UnlockOnConnection.Click += new System.EventHandler(this.chk_UnlockOnConnection_Click);
      // 
      // btn_Unlock
      // 
      this.btn_Unlock.Location = new System.Drawing.Point(581, 49);
      this.btn_Unlock.Margin = new System.Windows.Forms.Padding(7);
      this.btn_Unlock.Name = "btn_Unlock";
      this.btn_Unlock.Size = new System.Drawing.Size(261, 51);
      this.btn_Unlock.TabIndex = 109;
      this.btn_Unlock.Text = "Unlock Volume Now";
      this.btn_Unlock.UseVisualStyleBackColor = true;
      this.btn_Unlock.Click += new System.EventHandler(this.btn_Unlock_Click);
      // 
      // grp_Drive
      // 
      this.grp_Drive.Controls.Add(this.tx_Custom);
      this.grp_Drive.Controls.Add(this.lbl_Custom);
      this.grp_Drive.Controls.Add(this.btn_RefreshVolumes);
      this.grp_Drive.Controls.Add(this.btn_Clear);
      this.grp_Drive.Controls.Add(this.label1);
      this.grp_Drive.Controls.Add(this.cbx_SystemVolume);
      this.grp_Drive.Location = new System.Drawing.Point(21, 17);
      this.grp_Drive.Margin = new System.Windows.Forms.Padding(7);
      this.grp_Drive.Name = "grp_Drive";
      this.grp_Drive.Padding = new System.Windows.Forms.Padding(7);
      this.grp_Drive.Size = new System.Drawing.Size(1026, 285);
      this.grp_Drive.TabIndex = 112;
      this.grp_Drive.TabStop = false;
      this.grp_Drive.Text = "Drive info";
      // 
      // tx_Custom
      // 
      this.tx_Custom.Location = new System.Drawing.Point(130, 150);
      this.tx_Custom.Name = "tx_Custom";
      this.tx_Custom.Size = new System.Drawing.Size(879, 35);
      this.tx_Custom.TabIndex = 113;
      this.tx_Custom.TextChanged += new System.EventHandler(this.tx_Custom_TextChanged);
      this.tx_Custom.Validated += new System.EventHandler(this.tx_Custom_Validated);
      // 
      // lbl_Custom
      // 
      this.lbl_Custom.AutoSize = true;
      this.lbl_Custom.Location = new System.Drawing.Point(23, 153);
      this.lbl_Custom.Name = "lbl_Custom";
      this.lbl_Custom.Size = new System.Drawing.Size(101, 29);
      this.lbl_Custom.TabIndex = 112;
      this.lbl_Custom.Text = "Custom:";
      // 
      // btn_RefreshVolumes
      // 
      this.btn_RefreshVolumes.Location = new System.Drawing.Point(748, 42);
      this.btn_RefreshVolumes.Margin = new System.Windows.Forms.Padding(7);
      this.btn_RefreshVolumes.Name = "btn_RefreshVolumes";
      this.btn_RefreshVolumes.Size = new System.Drawing.Size(261, 51);
      this.btn_RefreshVolumes.TabIndex = 111;
      this.btn_RefreshVolumes.Text = "Refresh Volumes";
      this.btn_RefreshVolumes.UseVisualStyleBackColor = true;
      this.btn_RefreshVolumes.Visible = false;
      // 
      // btn_Clear
      // 
      this.btn_Clear.Location = new System.Drawing.Point(748, 214);
      this.btn_Clear.Margin = new System.Windows.Forms.Padding(7);
      this.btn_Clear.Name = "btn_Clear";
      this.btn_Clear.Size = new System.Drawing.Size(261, 51);
      this.btn_Clear.TabIndex = 110;
      this.btn_Clear.Text = "Clear KeeLocker";
      this.btn_Clear.UseVisualStyleBackColor = true;
      this.btn_Clear.Click += new System.EventHandler(this.btn_Clear_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(23, 56);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(171, 29);
      this.label1.TabIndex = 107;
      this.label1.Text = "Select volume:";
      // 
      // cbx_SystemVolume
      // 
      this.cbx_SystemVolume.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
      this.cbx_SystemVolume.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.cbx_SystemVolume.Location = new System.Drawing.Point(22, 97);
      this.cbx_SystemVolume.Margin = new System.Windows.Forms.Padding(7);
      this.cbx_SystemVolume.Name = "cbx_SystemVolume";
      this.cbx_SystemVolume.Size = new System.Drawing.Size(987, 36);
      this.cbx_SystemVolume.TabIndex = 106;
      this.cbx_SystemVolume.SelectedIndexChanged += new System.EventHandler(this.cbx_SystemVolume_SelectedIndexChanged);
      // 
      // grp_Unlock
      // 
      this.grp_Unlock.Controls.Add(this.icon);
      this.grp_Unlock.Controls.Add(this.chk_IsRecoveryKey);
      this.grp_Unlock.Controls.Add(this.chk_UnlockOnOpening);
      this.grp_Unlock.Controls.Add(this.chk_UnlockOnConnection);
      this.grp_Unlock.Controls.Add(this.btn_Unlock);
      this.grp_Unlock.Location = new System.Drawing.Point(21, 320);
      this.grp_Unlock.Margin = new System.Windows.Forms.Padding(7);
      this.grp_Unlock.Name = "grp_Unlock";
      this.grp_Unlock.Padding = new System.Windows.Forms.Padding(7);
      this.grp_Unlock.Size = new System.Drawing.Size(1029, 245);
      this.grp_Unlock.TabIndex = 111;
      this.grp_Unlock.TabStop = false;
      this.grp_Unlock.Text = "Unlock settings";
      // 
      // icon
      // 
      this.icon.InitialImage = null;
      this.icon.Location = new System.Drawing.Point(979, 194);
      this.icon.Name = "icon";
      this.icon.Size = new System.Drawing.Size(32, 32);
      this.icon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
      this.icon.TabIndex = 110;
      this.icon.TabStop = false;
      this.icon.Click += new System.EventHandler(this.icon_Click);
      // 
      // chk_IsRecoveryKey
      // 
      this.chk_IsRecoveryKey.AutoSize = true;
      this.chk_IsRecoveryKey.Location = new System.Drawing.Point(61, 194);
      this.chk_IsRecoveryKey.Margin = new System.Windows.Forms.Padding(7);
      this.chk_IsRecoveryKey.Name = "chk_IsRecoveryKey";
      this.chk_IsRecoveryKey.Size = new System.Drawing.Size(390, 33);
      this.chk_IsRecoveryKey.TabIndex = 108;
      this.chk_IsRecoveryKey.Text = "Use password as a recovery key";
      this.chk_IsRecoveryKey.UseVisualStyleBackColor = true;
      this.chk_IsRecoveryKey.Click += new System.EventHandler(this.chk_IsRecoveryKey_Click);
      // 
      // txt_Info
      // 
      this.txt_Info.AutoSize = true;
      this.txt_Info.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.txt_Info.Location = new System.Drawing.Point(0, 694);
      this.txt_Info.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
      this.txt_Info.Name = "txt_Info";
      this.txt_Info.Size = new System.Drawing.Size(75, 29);
      this.txt_Info.TabIndex = 108;
      this.txt_Info.Text = "status";
      // 
      // KeeLockerEntryTab
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.Controls.Add(this.txt_Info);
      this.Controls.Add(this.grp_Unlock);
      this.Controls.Add(this.grp_Drive);
      this.Margin = new System.Windows.Forms.Padding(7);
      this.Name = "KeeLockerEntryTab";
      this.Size = new System.Drawing.Size(1079, 723);
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
		private System.Windows.Forms.Label lbl_Custom;
	}
}