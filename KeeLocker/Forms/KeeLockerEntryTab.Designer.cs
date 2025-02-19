
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
            this.cbx_DriveMountPoint = new KeeLocker.Forms.RichComboBox();
            this.chk_UnlockOnOpening = new System.Windows.Forms.CheckBox();
            this.chk_UnlockOnConnection = new System.Windows.Forms.CheckBox();
            this.btn_Unlock = new System.Windows.Forms.Button();
            this.rdo_MountPoint = new System.Windows.Forms.RadioButton();
            this.rdo_DriveGUID = new System.Windows.Forms.RadioButton();
            this.cbx_DriveGUID = new KeeLocker.Forms.RichComboBox();
            this.grp_Drive = new System.Windows.Forms.GroupBox();
            this.btn_Clear = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.cbx_SystemVolume = new KeeLocker.Forms.RichComboBox();
            this.btn_DriveGUID = new System.Windows.Forms.Button();
            this.lbl_DriveGUID = new System.Windows.Forms.Label();
            this.grp_Unlock = new System.Windows.Forms.GroupBox();
            this.chk_IsRecoveryKey = new System.Windows.Forms.CheckBox();
            this.txt_Info = new System.Windows.Forms.Label();
            this.grp_Drive.SuspendLayout();
            this.grp_Unlock.SuspendLayout();
            this.SuspendLayout();
            // 
            // cbx_DriveMountPoint
            // 
            this.cbx_DriveMountPoint.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbx_DriveMountPoint.Location = new System.Drawing.Point(301, 119);
            this.cbx_DriveMountPoint.Margin = new System.Windows.Forms.Padding(7);
            this.cbx_DriveMountPoint.Name = "cbx_DriveMountPoint";
            this.cbx_DriveMountPoint.Size = new System.Drawing.Size(746, 36);
            this.cbx_DriveMountPoint.TabIndex = 102;
            this.cbx_DriveMountPoint.Validated += new System.EventHandler(this.cbx_DriveMountPoint_Validated);
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
            // rdo_MountPoint
            // 
            this.rdo_MountPoint.AutoSize = true;
            this.rdo_MountPoint.Location = new System.Drawing.Point(28, 119);
            this.rdo_MountPoint.Margin = new System.Windows.Forms.Padding(7);
            this.rdo_MountPoint.Name = "rdo_MountPoint";
            this.rdo_MountPoint.Size = new System.Drawing.Size(231, 33);
            this.rdo_MountPoint.TabIndex = 101;
            this.rdo_MountPoint.Text = "Drive mountpoint:";
            this.rdo_MountPoint.UseVisualStyleBackColor = true;
            this.rdo_MountPoint.Click += new System.EventHandler(this.rdo_MountPoint_Click);
            // 
            // rdo_DriveGUID
            // 
            this.rdo_DriveGUID.AutoSize = true;
            this.rdo_DriveGUID.Location = new System.Drawing.Point(28, 246);
            this.rdo_DriveGUID.Margin = new System.Windows.Forms.Padding(7);
            this.rdo_DriveGUID.Name = "rdo_DriveGUID";
            this.rdo_DriveGUID.Size = new System.Drawing.Size(170, 33);
            this.rdo_DriveGUID.TabIndex = 104;
            this.rdo_DriveGUID.Text = "Drive GUID:";
            this.rdo_DriveGUID.UseVisualStyleBackColor = true;
            this.rdo_DriveGUID.Click += new System.EventHandler(this.rdo_DriveGUID_Click);
            // 
            // cbx_DriveGUID
            // 
            this.cbx_DriveGUID.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbx_DriveGUID.Location = new System.Drawing.Point(301, 239);
            this.cbx_DriveGUID.Margin = new System.Windows.Forms.Padding(7);
            this.cbx_DriveGUID.Name = "cbx_DriveGUID";
            this.cbx_DriveGUID.Size = new System.Drawing.Size(746, 36);
            this.cbx_DriveGUID.TabIndex = 105;
            this.cbx_DriveGUID.Validated += new System.EventHandler(this.txt_DriveGUID_Validated);
            // 
            // grp_Drive
            // 
            this.grp_Drive.Controls.Add(this.btn_Clear);
            this.grp_Drive.Controls.Add(this.label1);
            this.grp_Drive.Controls.Add(this.cbx_SystemVolume);
            this.grp_Drive.Controls.Add(this.btn_DriveGUID);
            this.grp_Drive.Controls.Add(this.lbl_DriveGUID);
            this.grp_Drive.Controls.Add(this.cbx_DriveGUID);
            this.grp_Drive.Controls.Add(this.rdo_DriveGUID);
            this.grp_Drive.Controls.Add(this.rdo_MountPoint);
            this.grp_Drive.Controls.Add(this.cbx_DriveMountPoint);
            this.grp_Drive.Location = new System.Drawing.Point(21, 17);
            this.grp_Drive.Margin = new System.Windows.Forms.Padding(7);
            this.grp_Drive.Name = "grp_Drive";
            this.grp_Drive.Padding = new System.Windows.Forms.Padding(7);
            this.grp_Drive.Size = new System.Drawing.Size(1075, 406);
            this.grp_Drive.TabIndex = 112;
            this.grp_Drive.TabStop = false;
            this.grp_Drive.Text = "Drive info";
            // 
            // btn_Clear
            // 
            this.btn_Clear.Location = new System.Drawing.Point(786, 332);
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
            this.label1.Location = new System.Drawing.Point(28, 56);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 29);
            this.label1.TabIndex = 107;
            this.label1.Text = "Select:";
            // 
            // cbx_SystemVolume
            // 
            this.cbx_SystemVolume.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
            this.cbx_SystemVolume.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbx_SystemVolume.Location = new System.Drawing.Point(125, 56);
            this.cbx_SystemVolume.Margin = new System.Windows.Forms.Padding(7);
            this.cbx_SystemVolume.Name = "cbx_SystemVolume";
            this.cbx_SystemVolume.Size = new System.Drawing.Size(922, 36);
            this.cbx_SystemVolume.TabIndex = 106;
            this.cbx_SystemVolume.SelectedIndexChanged += new System.EventHandler(this.cbx_SystemVolume_SelectedIndexChanged);
            // 
            // btn_DriveGUID
            // 
            this.btn_DriveGUID.Location = new System.Drawing.Point(301, 169);
            this.btn_DriveGUID.Margin = new System.Windows.Forms.Padding(7);
            this.btn_DriveGUID.Name = "btn_DriveGUID";
            this.btn_DriveGUID.Size = new System.Drawing.Size(509, 56);
            this.btn_DriveGUID.TabIndex = 103;
            this.btn_DriveGUID.Text = "Convert mountpoint to GUID";
            this.btn_DriveGUID.UseVisualStyleBackColor = true;
            this.btn_DriveGUID.Click += new System.EventHandler(this.btn_DriveGUID_Click);
            // 
            // lbl_DriveGUID
            // 
            this.lbl_DriveGUID.Location = new System.Drawing.Point(131, 304);
            this.lbl_DriveGUID.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.lbl_DriveGUID.Name = "lbl_DriveGUID";
            this.lbl_DriveGUID.Size = new System.Drawing.Size(566, 58);
            this.lbl_DriveGUID.TabIndex = 104;
            this.lbl_DriveGUID.Text = "Use a GUID whenever possible, especially on removable devices.";
            // 
            // grp_Unlock
            // 
            this.grp_Unlock.Controls.Add(this.chk_IsRecoveryKey);
            this.grp_Unlock.Controls.Add(this.chk_UnlockOnOpening);
            this.grp_Unlock.Controls.Add(this.chk_UnlockOnConnection);
            this.grp_Unlock.Controls.Add(this.btn_Unlock);
            this.grp_Unlock.Location = new System.Drawing.Point(21, 437);
            this.grp_Unlock.Margin = new System.Windows.Forms.Padding(7);
            this.grp_Unlock.Name = "grp_Unlock";
            this.grp_Unlock.Padding = new System.Windows.Forms.Padding(7);
            this.grp_Unlock.Size = new System.Drawing.Size(1075, 245);
            this.grp_Unlock.TabIndex = 111;
            this.grp_Unlock.TabStop = false;
            this.grp_Unlock.Text = "Unlock settings";
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
            this.Size = new System.Drawing.Size(1127, 723);
            this.grp_Drive.ResumeLayout(false);
            this.grp_Drive.PerformLayout();
            this.grp_Unlock.ResumeLayout(false);
            this.grp_Unlock.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private RichComboBox cbx_DriveMountPoint;
		private System.Windows.Forms.CheckBox chk_UnlockOnOpening;
		private System.Windows.Forms.CheckBox chk_UnlockOnConnection;
		private System.Windows.Forms.Button btn_Unlock;
		private System.Windows.Forms.RadioButton rdo_MountPoint;
		private System.Windows.Forms.RadioButton rdo_DriveGUID;
		private RichComboBox cbx_DriveGUID;
		private System.Windows.Forms.GroupBox grp_Drive;
		private System.Windows.Forms.GroupBox grp_Unlock;
		private System.Windows.Forms.Button btn_DriveGUID;
		private System.Windows.Forms.Label lbl_DriveGUID;
		private System.Windows.Forms.CheckBox chk_IsRecoveryKey;
    private System.Windows.Forms.Label label1;
    private RichComboBox cbx_SystemVolume;
    private System.Windows.Forms.Label txt_Info;
    private System.Windows.Forms.Button btn_Clear;
  }
}
