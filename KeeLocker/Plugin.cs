﻿using KeeLocker.Forms;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace KeeLocker
{
	public partial class KeeLockerExt : KeePass.Plugins.Plugin
	{
		public const string StringName_DriveMountPoint = "KeeLockerMountPoint";
		public const string StringName_DriveGUID = "KeeLockerGUID";
		public const string StringName_DriveIdType = "KeeLockerType";
		public const string StringName_UnlockOnOpening = "KeeLockerOnOpening";
		public const string StringName_UnlockOnConnection = "KeeLockerOnConnection";
		public const string StringName_IsRecoveryKey = "KeeLockerIsRecoveryKey";

		public const string StringName_Password = "Password";
		internal KeePass.Plugins.IPluginHost m_host;
		internal FveApi.SSubscription m_Subscription;
		System.Drawing.Image m_Image = null;


		public override string UpdateUrl
		{
			get
			{
				return null; // "https://raw.github.com/Gugli/KeeLocker/main/VersionInfo.txt";
			}
		}

		public override Image SmallIcon
		{
			get
			{
				if (m_Image == null)
					m_Image = new System.Drawing.Bitmap(new System.IO.MemoryStream(Properties.Resources.KeeLocker));
				return m_Image;
			}
		}

		public override bool Initialize(KeePass.Plugins.IPluginHost host)
		{
			if (host == null)
				return false;
			m_host = host;

			// Signed update checks
			// KeePass.Util.UpdateCheckEx.SetFileSigKey(UpdateUrl, "<RSAKeyValue><Modulus>0N6jerZiraXQTGZ2kqbQHCOs1pjyFRmHwG6zVQwWQ5M0YONrT5nEJGBCOJ8gliJ+/ONerm8JfrB9eycsvq6cYNGC9WvGTVt81KDhnOlCSPdHkB3qtPU5Vin4UIFNjCmb0/Bnz7hyoVjACqNQUSeIWFSTPtNw2/H7EK+YZpGbdD540QxdRzZUWi50AxS1kCYUzvj1zYjuXBHw7YMP/GFQIuFBJrZUv1nQwVG1+j4u6aWe8wP5RXzm0LpdLtc9JeoVfP1DBujuugKxpOXXDzB+YPI5RIIAOEc3qd4BNZkLOU3JEdGu/MCWL7GgHQOlGjR+jWpKGGkUWFplkCA7YRtKAlRQRQY3Id9wKjinhTyhhZ7r9qkHK8m2dCVaL8F2dXj8KTSZZWIZHV56a6Kou2Kw0Vq9ra6Wt6uZH1lLX3h05ygDe3Gm5rxax150ScjQHBhHxTo03xzaif5AP1zW0eCeCDfH37dPjZBUQb/zEy0pqbKATwMAFdMLWKCS5hy+a5L5xhd+WIf0OW6AgapA4O/xFABucSFVh9Ugpzvy9j5Gb4+9+aygGlnktprZDBAI5t9QEZz8Vkjxv+nKplPPH37f01K7mIzSjsxnGmcBM4CFVPjfG0i9eAa+4pVqFgXaW3TNQjWON8sMrslCqaFB+0s79MuJbps2awevB+hyssCOacE=</Modulus><Exponent>AQAB</Exponent></RSAKeyValue>");

			// register FileOpened event : needed to open locked storages
			m_host.MainWindow.FileOpened += OnKPDBOpen;

			// register WindowAdded event : needed to add the options tab in the EditEntry window
			KeePass.UI.GlobalWindowManager.WindowAdded += OnWindowAdded;

			m_Subscription = FveApi.StateChangeNotification_Subscribe(OnDriveConnected);

			// host.CustomConfig.SetString("KeeLockeer_Hello", "Welcome");
			return true;
		}

		public override void Terminate()
		{
			FveApi.StateChangeNotification_Unsubscribe(m_Subscription);
			m_Subscription.Clear();

			// remove event listeners
			KeePass.UI.GlobalWindowManager.WindowAdded -= OnWindowAdded;

			m_host.MainWindow.FileOpened -= OnKPDBOpen;
		}

		public override System.Windows.Forms.ToolStripMenuItem GetMenuItem(KeePass.Plugins.PluginMenuType t)
		{
			if (t == KeePass.Plugins.PluginMenuType.Main)
			{
				System.Windows.Forms.ToolStripMenuItem UnlockThisDB = new System.Windows.Forms.ToolStripMenuItem();
				UnlockThisDB.Text = "KeeLocker unlock volumes in this DB";
				UnlockThisDB.Image = SmallIcon;
				UnlockThisDB.Click += this.UnlockThisDB;
				UnlockThisDB.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
				{
					bool DBIsOpen = ((m_host.MainWindow.ActiveDatabase != null) && m_host.MainWindow.ActiveDatabase.IsOpen);
					UnlockThisDB.Enabled = DBIsOpen;
				};
				return UnlockThisDB;
			}
			else if (t == KeePass.Plugins.PluginMenuType.Entry)
			{
				System.Windows.Forms.ToolStripMenuItem UnlockEntry = new System.Windows.Forms.ToolStripMenuItem();
				UnlockEntry.Click += this.UnlockEntries;
				UnlockEntry.Image = SmallIcon;
				UnlockEntry.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
				{
					KeePassLib.PwEntry[] Entries = m_host.MainWindow.GetSelectedEntries();
					int SelectedCount = Entries == null ? 0 : Entries.Length;
					UnlockEntry.Enabled = SelectedCount > 0;
					UnlockEntry.Text = SelectedCount > 1 ? "KeeLocker unlock volumes" : "KeeLocker unlock volume";
				};
				return UnlockEntry;
			}
			else if (t == KeePass.Plugins.PluginMenuType.Group)
			{
				System.Windows.Forms.ToolStripMenuItem UnlockGroup = new System.Windows.Forms.ToolStripMenuItem();
				UnlockGroup.Text = "KeeLocker unlock volumes in this group";
				UnlockGroup.Image = SmallIcon;
				UnlockGroup.Click += this.UnlockGroup;
				UnlockGroup.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
				{
					UnlockGroup.Enabled = m_host.MainWindow.GetSelectedGroup() != null;
				};
				return UnlockGroup;
			}
			else
			{
				return null;
			}
		}

		private void OnWindowAdded(object sender, KeePass.UI.GwmWindowEventArgs e)
		{
			if (e.Form.Name == "PwEntryForm")
			{
				var ef = e.Form as KeePass.Forms.PwEntryForm;
				if (ef != null)
				{
					ef.Shown += OnEntryFormShown;
					return;
				}
			}
		}

		private Type GetControl<Type>(KeePass.Forms.PwEntryForm Form, string Name) where Type : System.Windows.Forms.Control
		{
			System.Windows.Forms.Control[] Controls = Form.Controls.Find(Name, true);
			if (Controls.Length == 0)
				return default(Type);

			return Controls[0] as Type;
		}
		const string KeeLockerTabName = "KeeLockerTab";

		void OnEntryFormShown(object sender, EventArgs e)
		{
			KeePass.Forms.PwEntryForm Form = sender as KeePass.Forms.PwEntryForm;
			if (Form == null)
				return;

			KeePassLib.PwEntry Entry = Form.EntryRef;
			if (Entry == null)
				return;

			KeePassLib.Collections.ProtectedStringDictionary strings = Form.EntryStrings;
			if (strings == null)
				return;

			System.Windows.Forms.TabControl tabMain = GetControl<System.Windows.Forms.TabControl>(Form, "m_tabMain");
			System.Windows.Forms.Button btnOk = GetControl<System.Windows.Forms.Button>(Form, "m_btnOK");

			KeeLocker.Forms.KeeLockerEntryTab KeeLockerEntryTab = new KeeLocker.Forms.KeeLockerEntryTab(m_host, this, Entry, strings, Form);

			System.Windows.Forms.TabPage KeeLockerEntryTabContainer = new System.Windows.Forms.TabPage("KeeLocker");
			KeeLockerEntryTabContainer.Name = KeeLockerTabName;
			KeeLockerEntryTabContainer.Tag = KeeLockerEntryTab;
			KeeLockerEntryTabContainer.Controls.Add(KeeLockerEntryTab);
			KeeLockerEntryTab.Dock = System.Windows.Forms.DockStyle.Fill;
			tabMain.TabPages.Add(KeeLockerEntryTabContainer);

			btnOk.Click += KeeLockerEntryTab.OnSave;
			tabMain.SelectedIndexChanged += TabMain_SelectedIndexChanged;
		}

		private void TabMain_SelectedIndexChanged(object sender, EventArgs e)
		{
			System.Windows.Forms.TabControl tabMain = (System.Windows.Forms.TabControl)sender;
			int s = tabMain.SelectedIndex;
			for (int i = 0; i < tabMain.TabCount; i++)
			{
				System.Windows.Forms.TabPage tab = tabMain.TabPages[i];
				bool isKeeLockerTab = (tab != null && tab.Name == KeeLockerTabName && tab.Tag is KeeLocker.Forms.KeeLockerEntryTab);
				if (isKeeLockerTab)
				{
					((KeeLocker.Forms.KeeLockerEntryTab)tab.Tag).SetTabSelected(s == i);
				}
			}
		}



		private void OnKPDBOpen(object sender, KeePass.Forms.FileOpenedEventArgs e)
		{
			UnlockDatabase(e.Database, EUnlockReason.DatabaseOpening, null);
		}

		private void OnDriveConnected()
		{
			if (m_host.MainWindow.InvokeRequired)
			{
				// trampoline to run on main thread, avoid possible race condition
				m_host.MainWindow.Invoke(new Action(OnDriveConnected));
				return;
			}
			List<PwDatabase> OpenDatabases = m_host.MainWindow.DocumentManager.GetOpenDatabases();
			foreach (KeePassLib.PwDatabase Database in OpenDatabases)
			{
				UnlockDatabase(Database, EUnlockReason.DriveConnected, null);
			}
		}

		private void UnlockThisDB(object sender, EventArgs e)
		{
			UnlockDatabase(m_host.MainWindow.ActiveDatabase, EUnlockReason.DatabaseOpening, sender as ToolStripMenuItem);
		}

		private void UnlockGroup(object sender, EventArgs e)
		{
			UnlockGroup(m_host.MainWindow.GetSelectedGroup(), EUnlockReason.UserRequest, sender as ToolStripMenuItem);
		}

		private void UnlockEntries(object sender, EventArgs e)
		{
			KeePassLib.PwEntry[] Entries = m_host.MainWindow.GetSelectedEntries();
			if (Entries == null) return;
			IList<BitLockerItem> unlockset = mapUnlockItems(Entries, EUnlockReason.UserRequest);
			if (unlockset.Count == 0)
				return;
			System.Windows.Forms.ToolStripMenuItem UnlockEntry = sender as System.Windows.Forms.ToolStripMenuItem;
			if (UnlockEntry != null) UnlockEntry.Enabled = false;
			Common.UnlockBitLocker(unlockset, EUnlockReason.UserRequest, m_host.MainWindow, (bool success) =>
		  {
			  if (UnlockEntry != null) UnlockEntry.Enabled = true;
		  });
		}

		private void UnlockDatabase(KeePassLib.PwDatabase Database, EUnlockReason UnlockReason, ToolStripMenuItem sender)
		{
			UnlockGroup(Database.RootGroup, UnlockReason, sender);
		}

		private void UnlockGroup(KeePassLib.PwGroup Group, EUnlockReason UnlockReason, ToolStripMenuItem sender)
		{
			if (Group == null) return;
			KeePassLib.Collections.PwObjectList<KeePassLib.PwEntry> AllEntries = Group.GetEntries(true);
			IList<BitLockerItem> unlockset = mapUnlockItems(AllEntries, UnlockReason);
			if (unlockset.Count == 0)
				return;
			if (sender != null) sender.Enabled = false;
			Common.UnlockBitLocker(unlockset, UnlockReason, m_host.MainWindow, (bool success) =>
			 {
				 if (sender != null) sender.Enabled = true;

				 if (UnlockReason == EUnlockReason.DatabaseOpening || UnlockReason == EUnlockReason.DriveConnected)
				 {
					 string info = "KeeLogger unlocked volumes";
					 if (-1 == m_host.MainWindow.Text.IndexOf(info))
					 {
						 m_host.MainWindow.Text = m_host.MainWindow.Text + " - " + info;
						 ShowBalloonNotification(info);
					 }
				 }

			 });
		}

		private void ShowBalloonNotification(string info)
		{
			m_host.MainWindow.MainNotifyIcon.ShowBalloonTip(
			  5000, "KeeLocker",
			  info, ToolTipIcon.Info);
		}

		private IList<BitLockerItem> mapUnlockItems(IEnumerable<KeePassLib.PwEntry> pwEntries, EUnlockReason UnlockReason)
		{
			List<BitLockerItem> mapped = new List<BitLockerItem>();
			foreach (KeePassLib.PwEntry Entry in pwEntries)
			{
				if (Entry == null) continue;
				KeePassLib.Collections.ProtectedStringDictionary Strings = Entry.Strings;
				KeePassLib.Security.ProtectedString UnlockOnOpening = Strings.Get(StringName_UnlockOnOpening);
				KeePassLib.Security.ProtectedString UnlockOnConnection = Strings.Get(StringName_UnlockOnConnection);
				bool UnlockOnOpening_bool = Common.GetBoolSetting(UnlockOnOpening, Common.DefaultUnlockOnOpening);
				bool UnlockOnConnection_bool = Common.GetBoolSetting(UnlockOnConnection, Common.DefaultUnlockOnConnection);

				switch (UnlockReason)
				{
					case EUnlockReason.DatabaseOpening:
						if (!UnlockOnOpening_bool) continue;
						break;
					case EUnlockReason.DriveConnected:
						if (!UnlockOnConnection_bool) continue;
						break;
					case EUnlockReason.UserRequest:
						break;
				}

				KeePassLib.Security.ProtectedString DriveMountPoint = Strings.Get(StringName_DriveMountPoint);
				KeePassLib.Security.ProtectedString DriveGUID = Strings.Get(StringName_DriveGUID);
				if (((DriveMountPoint == null || DriveMountPoint.IsEmpty) && (DriveGUID == null || DriveGUID.IsEmpty)))
					continue;

				KeePassLib.Security.ProtectedString DriveIdTypeStr = Strings.Get(StringName_DriveIdType);
				KeePassLib.Security.ProtectedString IsRecoveryKey = Strings.Get(StringName_IsRecoveryKey);
				KeePassLib.Security.ProtectedString Password = Strings.Get(StringName_Password);
				bool IsRecoveryKey_bool = Common.GetBoolSetting(IsRecoveryKey, Common.DefaultIsRecoveryKey);

				if (Password == null || Password.IsEmpty)
					continue;
				EDriveIdType DriveIdType = Forms.KeeLockerEntryTab.GetDriveIdTypeFromString(DriveIdTypeStr);
				mapped.Add(new BitLockerItem(DriveIdType, DriveMountPoint, DriveGUID, Password, IsRecoveryKey_bool));
			}
			return mapped;
		}
	}
}
