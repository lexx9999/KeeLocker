using KeeLocker.BitLockerWMI;
using KeeLocker.Forms;
using KeePassLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace KeeLocker
{
	public partial class KeeLockerExt : KeePass.Plugins.Plugin
	{
		public const string StringName_DriveMountPoint = KeeLocker.Globals.CONFIG_PREFIX + "MountPoint";
		public const string StringName_DriveGUID = KeeLocker.Globals.CONFIG_PREFIX + "GUID";
		public const string StringName_DriveIdType = KeeLocker.Globals.CONFIG_PREFIX + "Type";
		public const string StringName_UnlockOnOpening = KeeLocker.Globals.CONFIG_PREFIX + "OnOpening";
		public const string StringName_UnlockOnConnection = KeeLocker.Globals.CONFIG_PREFIX + "OnConnection";
		public const string StringName_IsRecoveryKey = KeeLocker.Globals.CONFIG_PREFIX + "IsRecoveryKey";
		public const string StringName_RecoveryKey = "RecoveryKey";

		internal KeePass.Plugins.IPluginHost m_host;
		internal FveApi.SSubscription m_Subscription;
		System.Drawing.Image m_Image = null;


		public override string UpdateUrl
		{
			get
			{
				return KeeLocker.Globals.UPDATE_URL;
			}
		}

		public override Image SmallIcon
		{
			get
			{
				if (m_Image == null)
					m_Image = new System.Drawing.Bitmap(new System.IO.MemoryStream(Properties.Resources.AppIconBitmap));
				return m_Image;
			}
		}

		public override bool Initialize(KeePass.Plugins.IPluginHost host)
		{
			if (host == null)
				return false;
			m_host = host;

			// Signed update checks
			if (UpdateUrl != null && KeeLocker.Globals.FILE_SIGN_KEY != null)
				KeePass.Util.UpdateCheckEx.SetFileSigKey(UpdateUrl, KeeLocker.Globals.FILE_SIGN_KEY);

			// register FileOpened event : needed to open locked storages
			m_host.MainWindow.FileOpened += OnKPDBOpen;

			// register WindowAdded event : needed to add the options tab in the EditEntry window
			KeePass.UI.GlobalWindowManager.WindowAdded += OnWindowAdded;

			m_Subscription = FveApi.StateChangeNotification_Subscribe(OnDriveConnected);

			//bool runScanAdmin = false;
			//foreach (KeyValuePair<string, string> kv in host.CommandLineArgs.Parameters)
			//{

			// if (kv.Key.Equals(KeeLocker.Globals.APP_NAME+"Scan", StringComparison.InvariantCultureIgnoreCase))
			//	{
			//		runScanAdmin = true;
			//		break;
			//	}
			//}
			//if (runScanAdmin)
			//{

			//}


			// host.CustomConfig.SetString("KeeLocker_Hello", "Welcome");
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
			switch (t)
			{
				case KeePass.Plugins.PluginMenuType.Main:
					return createAppMenu(createUnlockThisDBMenuItem(),  createSearchVolumesMenuItem() , createOpenHomeMenuItem());
				case KeePass.Plugins.PluginMenuType.Group:
					return createAppMenu(createUnlockGroupMenuItem());
				case KeePass.Plugins.PluginMenuType.Entry:
					return createAppMenu(createUnlockEntryMenuItem());
				default:
					return null;
			}
		}

		private ToolStripMenuItem createAppMenu(params ToolStripMenuItem[] items)
		{
			if (items == null || items.Length == 0)
				return null;

			System.Windows.Forms.ToolStripMenuItem AppSubMenu = new System.Windows.Forms.ToolStripMenuItem();
			AppSubMenu.Text = KeeLocker.Globals.APP_NAME;
			AppSubMenu.Image = SmallIcon;
			foreach (ToolStripMenuItem item in items) if(item!=null)
			{
				AppSubMenu.DropDownItems.Add(item);
				if (item.Image == SmallIcon)
					item.Image = null;
				string t = item.Text;
				if (t.StartsWith(KeeLocker.Globals.APP_NAME + " "))
				{
					t = t.Substring(KeeLocker.Globals.APP_NAME.Length + 1);
					t = t.Substring(0, 1).ToUpper() + t.Substring(1);
					item.Text = t;
				}
				
			}
			return AppSubMenu;
		}


		private ToolStripMenuItem createUnlockGroupMenuItem()
		{
			System.Windows.Forms.ToolStripMenuItem UnlockGroup = new System.Windows.Forms.ToolStripMenuItem();
			UnlockGroup.Text = "Unlock volumes in this group";
			UnlockGroup.Click += this.UnlockGroup;
			UnlockGroup.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
			{
				UnlockGroup.Enabled = m_host.MainWindow.GetSelectedGroup() != null;
			};
			return UnlockGroup;
		}
		private ToolStripMenuItem createOpenHomeMenuItem()
		{
			System.Windows.Forms.ToolStripMenuItem OpenHomeUrl = new System.Windows.Forms.ToolStripMenuItem();
			OpenHomeUrl.Text = "Open "+Globals.APP_NAME+" Homepage";
			OpenHomeUrl.Click += delegate (object sender, EventArgs e)
			{
				OpenHomepage();
			};
			return OpenHomeUrl;
		}

		public void OpenHomepage()
		{
			Process.Start(new ProcessStartInfo { FileName = Globals.HOME_URL, UseShellExecute = true });
		}

		private ToolStripMenuItem createSearchVolumesMenuItem()
		{
			System.Windows.Forms.ToolStripMenuItem ScanConnected = new System.Windows.Forms.ToolStripMenuItem();
			ScanConnected.Text = string.Format("Search BitLocker volumes (Admin-only) into '{0}'", VolumeSearchResultGroup);
			ScanConnected.Click += this.ScanConnectedVolumes;
			ScanConnected.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
			{
				ScanConnected.Enabled = m_host.MainWindow.GetSelectedGroup() != null;
			};
			return ScanConnected;
		}

		private ToolStripMenuItem createUnlockEntryMenuItem()
		{
			System.Windows.Forms.ToolStripMenuItem UnlockEntry = new System.Windows.Forms.ToolStripMenuItem();
			UnlockEntry.Click += this.UnlockEntries;
			UnlockEntry.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
			{
				KeePassLib.PwEntry[] Entries = m_host.MainWindow.GetSelectedEntries();
				int SelectedCount = Entries == null ? 0 : Entries.Length;
				UnlockEntry.Enabled = SelectedCount > 0;
				UnlockEntry.Text = SelectedCount > 1 ? "Unlock volumes" : "Unlock volume";
			};
			return UnlockEntry;
		}

		private ToolStripMenuItem createUnlockThisDBMenuItem()
		{
			System.Windows.Forms.ToolStripMenuItem UnlockThisDB = new System.Windows.Forms.ToolStripMenuItem();
			UnlockThisDB.Text = "Unlock volumes in this DB";
			UnlockThisDB.Click += this.UnlockThisDB;
			UnlockThisDB.Paint += delegate (object sender, System.Windows.Forms.PaintEventArgs e)
			{
				bool DBIsOpen = ((m_host.MainWindow.ActiveDatabase != null) && m_host.MainWindow.ActiveDatabase.IsOpen);
				UnlockThisDB.Enabled = DBIsOpen;
			};
			return UnlockThisDB;
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
		const string KeeLockerTabName = KeeLocker.Globals.APP_NAME + "Tab";
		private const string VolumeSearchResultGroup = Globals.APP_NAME + "-VolumeSearch";

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

			System.Windows.Forms.TabPage KeeLockerEntryTabContainer = new System.Windows.Forms.TabPage(KeeLocker.Globals.APP_NAME);
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


		public void CreateUpdateScanResultsGroup(System.Collections.Generic.List<BitLockerWMI.VolumeInfo> volumeList, string groupName)
		{
			PwGroup scanResultGroup = m_host.Database.RootGroup.FindCreateGroup(groupName, true);

			foreach (var volume in volumeList)
			{
				if (volume.EncryptionMethod == 0)
					continue;

				string recoveryKey = null;
				bool password = false;

				if (volume.KeyProtectors != null)
				{
					foreach (var protector in volume.KeyProtectors)
					{
						switch (protector.Type)
						{
							case ProtectorType.NumericalPassword:
								recoveryKey = protector.NumericalPassword;
								break;
							case ProtectorType.Passphrase:
								password = true;
								break;
						}

					}
				}

				var M = Common.volumeRx.Match(volume.VolumeID);
				if (M.Success)
				{
					volume.VolumeID = @"\\?\" + M.Groups[1] + @"\";
				}
				M = Common.driveRx.Match(volume.DriveLetter);
				if (M.Success)
				{
					volume.DriveLetter = M.Groups[1].Value.ToUpperInvariant() + @"\";
				}
				VolumeInfo vi = new VolumeInfo { MountPoint = volume.DriveLetter, Volume = volume.VolumeID, DriveIdType = EDriveIdType.GUID };
				if (!string.IsNullOrEmpty(volume.DriveLetter))
				{
					try
					{
						vi.DriveInfo = new DriveInfo(volume.DriveLetter);
					}
					catch { }
				}


				var pe = new KeePassLib.PwEntry(true, true);
				EDriveIdType driveIdType;

				if (!string.IsNullOrEmpty(volume.VolumeID))
					driveIdType = EDriveIdType.GUID;
				else if (!string.IsNullOrEmpty(volume.DriveLetter))
					driveIdType = EDriveIdType.MountPoint;
				else
					driveIdType = Common.DriveIdTypeDefault;

				pe.Strings.Set(PwDefs.UserNameField, new KeePassLib.Security.ProtectedString(false, volume.PersistentVolumeID ?? volume.VolumeID ?? volume.DriveLetter));

				if (recoveryKey != null && password)
				{
					// pe.Strings.Set(KeeLockerExt.StringName_Password, null);
					pe.Strings.Set(KeeLockerExt.StringName_RecoveryKey, new KeePassLib.Security.ProtectedString(true, recoveryKey));
					pe.Strings.Set(KeeLockerExt.StringName_IsRecoveryKey, new KeePassLib.Security.ProtectedString(false, Common.BoolFor(false, true)));
					pe.Strings.Set(PwDefs.NotesField, new KeePassLib.Security.ProtectedString(false, string.Format("Recovery Key stored in field '{0}'", KeeLockerExt.StringName_RecoveryKey) ));
				}
				else if (recoveryKey != null)
				{
					// only recovery key, store as password
					pe.Strings.Set(PwDefs.PasswordField, new KeePassLib.Security.ProtectedString(true, recoveryKey));
					pe.Strings.Set(KeeLockerExt.StringName_IsRecoveryKey, new KeePassLib.Security.ProtectedString(false, Common.BoolFor(true, false)));

				}
				else
				{
					pe.Strings.Set(KeeLockerExt.StringName_IsRecoveryKey, new KeePassLib.Security.ProtectedString(false, Common.BoolFor(false, true)));
				}
				if (password)
				{
					// add note to fill password manually
					pe.Strings.Set(PwDefs.NotesField, new KeePassLib.Security.ProtectedString(false, "Password needs to be filled manually"));
				}

				pe.Strings.Set(KeeLockerExt.StringName_DriveIdType, new KeePassLib.Security.ProtectedString(false, driveIdType == Common.DriveIdTypeDefault ? "" : driveIdType.ToString()));
				pe.Strings.Set(KeeLockerExt.StringName_DriveGUID, new KeePassLib.Security.ProtectedString(false, volume.VolumeID));
				pe.Strings.Set(KeeLockerExt.StringName_DriveMountPoint, new KeePassLib.Security.ProtectedString(false, volume.DriveLetter));

				pe.Strings.Set(PwDefs.TitleField, new KeePassLib.Security.ProtectedString(false, "BitLocker "+vi.DisplayText));
				scanResultGroup.AddEntry(pe, true);

			}
			m_host.MainWindow.UpdateUI(false, null, true, scanResultGroup, true, scanResultGroup, true);
		}

		private void ScanConnectedVolumes(object sender, EventArgs e)
		{
			if (!BitLocker.IsAdministrator())
			{
				ShowBalloonNotification("KeePass must be started as Administrator to allow scanning for volumes!");
				return;
			}

			var volumeList = BitLocker.GetBitLockerVolumes();
			bool hasEncrypted=volumeList.Find(v => v.EncryptionMethod != 0) !=null;
			
			if (!hasEncrypted)
			{
				ShowBalloonNotification("No encrypted volumes found");
				return;
			}
			string searchGroupName = VolumeSearchResultGroup;
		
			CreateUpdateScanResultsGroup(volumeList,searchGroupName);
            ShowBalloonNotification(string.Format("Encrypted volume entries added to '{0}' (if any)", searchGroupName));
			
			if (Debugger.IsAttached)
			{
				KeeLockerScanResults scanResults = new KeeLockerScanResults(m_host, this, volumeList);
				KeePass.UI.UIUtil.ShowDialogAndDestroy(scanResults);
			}
		}

		private void UnlockThisDB(object sender, EventArgs e)
		{
			UnlockDatabase(m_host.MainWindow.ActiveDatabase, EUnlockReason.UserRequest, sender as ToolStripMenuItem);
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
			Common.UnlockBitLocker(unlockset, EUnlockReason.UserRequest, m_host.MainWindow, (long SucceededCount, long AttemptedCount) =>
		  {
			  if (UnlockEntry != null) UnlockEntry.Enabled = true;
			  reportUnlockStatus(SucceededCount, AttemptedCount);
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
			Common.UnlockBitLocker(unlockset, UnlockReason, m_host.MainWindow, (long SucceededCount, long AttemptedCount) =>
			{
				if (sender != null) sender.Enabled = true;
				reportUnlockStatus(SucceededCount, AttemptedCount);
			});
		}

		private void reportUnlockStatus(long SucceededCount, long AttemptedCount)
		{
			if (AttemptedCount > 0)
			{
				string info = (SucceededCount < AttemptedCount) ? string.Format("{0} unlocked {1} of {2} volumes", KeeLocker.Globals.APP_NAME, SucceededCount, AttemptedCount) : KeeLocker.Globals.APP_NAME + " unlocked volumes";
				if (-1 == m_host.MainWindow.Text.IndexOf(info))
				{
					m_host.MainWindow.Text = m_host.MainWindow.Text + " - " + info;
					ShowBalloonNotification(info);
				}
			}
		}

		private void ShowBalloonNotification(string info)
		{
			m_host.MainWindow.MainNotifyIcon.ShowBalloonTip(
			  5000, KeeLocker.Globals.APP_NAME,
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
				KeePassLib.Security.ProtectedString Password = Strings.Get(PwDefs.PasswordField);
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
