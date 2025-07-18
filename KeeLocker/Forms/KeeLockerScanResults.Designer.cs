namespace KeeLocker.Forms
{
	partial class KeeLockerScanResults
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
      this.tx_Scan = new System.Windows.Forms.TextBox();
      this.SuspendLayout();
      // 
      // tx_Scan
      // 
      this.tx_Scan.AcceptsReturn = true;
      this.tx_Scan.AcceptsTab = true;
      this.tx_Scan.Dock = System.Windows.Forms.DockStyle.Fill;
      this.tx_Scan.Location = new System.Drawing.Point(0, 0);
      this.tx_Scan.Multiline = true;
      this.tx_Scan.Name = "tx_Scan";
      this.tx_Scan.ReadOnly = true;
      this.tx_Scan.ScrollBars = System.Windows.Forms.ScrollBars.Both;
      this.tx_Scan.Size = new System.Drawing.Size(800, 450);
      this.tx_Scan.TabIndex = 0;
      this.tx_Scan.WordWrap = false;
      // 
      // KeeLockerScanResults
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(800, 450);
      this.Controls.Add(this.tx_Scan);
      this.Name = "KeeLockerScanResults";
      this.Text = "KeeLocker-Volume-Search";
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox tx_Scan;
	}
}