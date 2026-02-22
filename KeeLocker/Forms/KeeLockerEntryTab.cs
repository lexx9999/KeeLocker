using KeePass.Forms;
using KeePass.Plugins;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace KeeLocker.Forms
{
	public partial class KeeLockerEntryTab : UserControl
	{
		private KeePass.Plugins.IPluginHost m_host;
		private KeeLockerExt m_plugin;
		private KeePassLib.PwEntry m_entry;
		private KeePassLib.Collections.ProtectedStringDictionary m_entrystrings;
		private readonly PwEntryForm PwEntryForm;
		private EntryData m_EntryData = new EntryData();
	


		private IList<VolumeInfo> VolumeInfos = new List<VolumeInfo>();

		public KeeLockerEntryTab(IPluginHost host, KeeLockerExt plugin, KeePassLib.PwEntry entry, KeePassLib.Collections.ProtectedStringDictionary strings, KeePass.Forms.PwEntryForm form)
		{
			m_host = host;
			m_plugin = plugin;
			m_entry = entry;
			m_entrystrings = strings;
			PwEntryForm = form;
			InitializeComponent();
			cbx_SystemVolume.ActiveShift = 0;
			icon.Image = plugin.SmallIcon;

			m_ComputerName = Environment.MachineName;
			m_MachineId = Common.GetMachineGuid();

			SetStatus(null);

			MigrateSettings();


			UpdateUi();

			if (!OS.IsWindows)
			{
				btn_Unlock.Enabled = false;
			}
		}

		private void MigrateSettings()
		{
			var e =Config.LoadKeelockerStringConfig(m_entrystrings);
			if (e != null)
			{
				// clear legacy settings
				SetStringValue(KeeLockerExt.StringName_DriveIdType, null);
				SetStringValue(KeeLockerExt.StringName_DriveMountPoint, null);
				SetStringValue(KeeLockerExt.StringName_DriveGUID, null);
				SetStringValue(KeeLockerExt.StringName_UnlockOnOpening, null);
				SetStringValue(KeeLockerExt.StringName_UnlockOnConnection, null);
				SetStringValue(KeeLockerExt.StringName_IsRecoveryKey, null);
				Config.SaveEntryData(m_entrystrings, e);
				PwEntryForm.UpdateEntryStrings(false, false);
			}
		}

		public static IList<VolumeInfo> EnumVolumeInfo()
		{
			if (OS.IsWindows)
			{
				return EnumVolumeInfoWin();
			}
			return new List<VolumeInfo>();
		}

		public static IList<VolumeInfo> EnumVolumeInfoWin()
		{
			IList<VolumeInfo> volumeInfo = new List<VolumeInfo>();

			int M = 1024;
			IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
			StringBuilder sb = new StringBuilder(M);
			IntPtr H = FveApi.FindFirstVolume(sb, (uint)sb.Capacity);
			if (H != INVALID_HANDLE_VALUE)
			{
				do
				{
					string vol = sb.ToString();

					string vshort;
					if (vol.StartsWith("\\\\?\\") && vol.EndsWith("\\"))
					{
						vshort = vol.Substring(4, vol.Length - 1 - 4);
					}
					else
					{
						vshort = vol;
					}
					sb.Clear();
					uint len = FveApi.QueryDosDevice(vshort, sb, (uint)sb.Capacity);

					string disk = (len > 0) ? sb.ToString() : null;
					List<string> tmp = null;
					bool added = false;
					if (FveApi.GetVolumePathNamesForVolumeName(vol, out tmp) && tmp.Count > 0)
					{
						foreach (string dl in tmp)
						{
							added = true;
							VolumeInfo vi = new VolumeInfo { MountPoint = dl, Volume = vol, DriveIdType = EDriveIdType.GUID };
							volumeInfo.Add(vi);
						}
					}
					if (!added)
					{
						VolumeInfo vi = new VolumeInfo { MountPoint = null, Volume = vol, DriveIdType = EDriveIdType.GUID };
						volumeInfo.Add(vi);
					}


					sb.Clear();

				} while (FveApi.FindNextVolume(H, sb, (uint)sb.Capacity));
				FveApi.FindVolumeClose(H);

			}

			foreach (System.IO.DriveInfo di in System.IO.DriveInfo.GetDrives())
			{
				switch (di.DriveType)
				{
					case System.IO.DriveType.Fixed:
					case System.IO.DriveType.Removable:
					case System.IO.DriveType.Ram:
						{
							bool used = false;
							foreach (VolumeInfo vi in volumeInfo)
							{
								if (string.Equals(di.Name,vi.MountPoint, StringComparison.InvariantCultureIgnoreCase))
								{
									used = true;
									vi.DriveInfo = di;
									break;
								}
							}
							if (!used)
							{
								volumeInfo.Add(new VolumeInfo
								{
									DriveInfo = di,
									MountPoint = di.Name,
									Volume = null,
									DriveIdType = EDriveIdType.MountPoint
								});

							}
						}
						break;
				}
			}
			return volumeInfo;
		}

		private void SettingsLoad()
		{

			m_EntryData = Config.LoadEntryData(m_entrystrings);
			if (m_EntryData == null) m_EntryData = new EntryData();
			UpdateUi();
			if (m_EntryData.Mounts.Count == 0)
			{
				SetStatus("Select drive from list or edit custom. Then Add to the matches list.", false);
			}
		}

		private Tuple<VolumeInfo, int> FindMountInfoMatch(EntryData entryData)
		{
			Tuple<VolumeInfo, int> viVolumeMachine = null;
			Tuple<VolumeInfo, int> viVolume = null;
			for (int m = 0; m < entryData.Mounts.Count; m++)
			{
				var mi = entryData.Mounts[m];
				if (string.IsNullOrEmpty(mi.DriveGUID))
					continue;
				foreach (var vi in VolumeInfos)
				{
					if (string.Equals(mi.DriveGUID, vi.Volume, StringComparison.InvariantCultureIgnoreCase))
					{
						// volume id equal
						if (string.Equals(mi.MachineId, m_MachineId, StringComparison.InvariantCultureIgnoreCase))
						{
							if (viVolumeMachine == null)
							{
								viVolumeMachine = new Tuple<VolumeInfo, int>(vi, m);
								break;
							}
						}
						else
						{
							if (viVolume == null)
							{
								viVolume = new Tuple<VolumeInfo, int>(vi, m);
								break;
							}
						}

					}
				}
			}
			if (viVolumeMachine != null)
				return viVolumeMachine;
			return viVolume;
		}


		private void FillVolumes()
		{
			this.cbx_SystemVolume.Items.Clear();
			VolumeInfos = EnumVolumeInfo();
			this.cbx_SystemVolume.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("(None/Custom)", RichComboBox.EItemType.Active, new VolumeInfo
			{
				MountPoint = "",
				Volume = "",
				DriveIdType = Common.DriveIdTypeDefault
			}));

			SortedSet<string> vs = new SortedSet<string>();
			SortedDictionary<string, VolumeInfo> sv = new SortedDictionary<string, VolumeInfo>();


			foreach (VolumeInfo vi in this.VolumeInfos)
			{
				sv.Add(vi.DisplayText, vi);

				if (!string.IsNullOrWhiteSpace(vi.Volume))
					vs.Add(vi.Volume);
			}
			foreach (KeyValuePair<string, VolumeInfo> kv in sv)
			{
				this.cbx_SystemVolume.Item_Add(new KeeLocker.Forms.RichComboBox.SItem(kv.Key, RichComboBox.EItemType.Active, kv.Value));
			}
		}

		private void SetStringValue(string SettingName, string SettingValue, bool protect = false)
		{
			Config.SetStringValue(m_entrystrings, SettingName, SettingValue, protect);
		}

		private void SettingsSave()
		{
			Config.SaveEntryData(m_entrystrings,m_EntryData);
		}



		private void UpdateUi()
		{
			chk_UnlockOnOpening.Checked = m_EntryData.UnlockOnOpening;
			chk_UnlockOnConnection.Checked = m_EntryData.UnlockOnConnection;
			chk_IsRecoveryKey.Checked = m_EntryData.PasswordIsRecoveryKey;
			var vv = FindMountInfoMatch(m_EntryData);
			if (vv != null)
				m_EntryData.SelectedMount = vv.Item2;

			lvMounts.Items.Clear();
			for (int z = 0; z < m_EntryData.Mounts.Count; z++)
			{
				var mi = m_EntryData.Mounts[z];
				var lvi = new ListViewItem();
				lvi.Tag = mi;
				lvi.Text = mi.DriveGUID;
				lvi.SubItems.Add(mi.DriveMountPoint);
				lvi.SubItems.Add(mi.ComputerName);
				lvi.SubItems.Add(mi.MachineId);
				lvi.SubItems.Add(mi.DriveIdType.ToString());
				lvMounts.Items.Add(lvi);
				lvi.Selected = m_EntryData.SelectedMount == z;

			}

			if (vv != null || cbx_SystemVolume.Items.Count == 0)
			{
				cbx_SystemVolume.SelectedData = (vv == null) ? null : vv.Item1;
			}
			else
			{
				cbx_SystemVolume.SelectedIndex = 0;
			}

			cbx_SystemVolume_SelectedIndexChanged(null, EventArgs.Empty);
			lvMounts_SelectedIndexChanged(null, EventArgs.Empty);
		}

		public void OnSave(object sender, EventArgs e)
		{
			if (_selected) // if not selected it was never opened or is already saved because of tab switch
				SettingsSave();
		}


		private void chk_UnlockOnOpening_Click(object sender, EventArgs e)
		{
			m_EntryData.UnlockOnOpening = chk_UnlockOnOpening.Checked;
			UpdateUi();
			SetStatus(null);

		}
		private void chk_UnlockOnConnection_Click(object sender, EventArgs e)
		{
			m_EntryData.UnlockOnConnection = chk_UnlockOnConnection.Checked;
			UpdateUi();
		}

		private void SetStatus(string text, bool isError = false)
		{
			if (string.IsNullOrEmpty(text))
			{
				txt_Info.Visible = false;
				txt_Info.Text = "";
			}
			else
			{
				txt_Info.Text = text;

				txt_Info.ForeColor = isError ? Color.Red : SystemColors.WindowText;
				txt_Info.Visible = true;
			}
		}

		private void btn_Unlock_Click(object sender, EventArgs e)
		{
			KeePassLib.Collections.ProtectedStringDictionary Strings = m_entrystrings;
			// KeePassLib.Security.ProtectedString IsRecoveryKey = Strings.Get(KeeLockerExt.StringName_IsRecoveryKey);
			this.btn_Unlock.Enabled = false;
			SetStatus("Unlocking...");
			List<BitLockerItem> mapped = new List<BitLockerItem>();
			Common.MapMountInfoToBitlocker(mapped, m_ComputerName, m_MachineId, m_entrystrings, m_EntryData);

			Common.UnlockBitLocker(mapped, EUnlockReason.UserRequest, this, (long SucceededCount, long AttemptedCount) =>
			{
				this.btn_Unlock.Enabled = true;
				if (AttemptedCount == 0)
				{
					SetStatus("Nothing to unlock!", true);
					return;
				}

				if (AttemptedCount == SucceededCount)
				{
					SetStatus("Successfully unlocked");
				}
				else SetStatus("Failed to unlock!", true);

				if (SucceededCount > 0)
					RefreshVolumes();
			});
		}

		private void RefreshVolumes()
		{
			FillVolumes();
			UpdateUi();
		}
		private void chk_IsRecoveryKey_Click(object sender, EventArgs e)
		{
			m_EntryData.PasswordIsRecoveryKey = chk_IsRecoveryKey.Checked;
			UpdateUi();
		}

		private void cbx_SystemVolume_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cbx_SystemVolume.Tag == null)
				cbx_SystemVolume.Tag = cbx_SystemVolume.Width;

			var vi = (VolumeInfo)cbx_SystemVolume.SelectedData;
			bool bShowCustom = vi == null || cbx_SystemVolume.SelectedIndex == 0;
			bool bAddEnabled = (cbx_SystemVolume.SelectedIndex > 0) // regular item
				|| (bShowCustom && null != CustomMountInfo()); // valid custom item

			btn_Add.Enabled = bAddEnabled;
			btn_AddMachine.Enabled = bAddEnabled;

			tx_Custom.Visible = bShowCustom;
			//lbl_Custom.Visible = bShowCustom;
			tx_Custom.Enabled = false;
			tx_Custom.Text = (bShowCustom && vi != null) ? vi.CustomText : "";
			tx_Custom.Enabled = bShowCustom;
			cbx_SystemVolume.Width = bShowCustom ? (int)cbx_SystemVolume.Tag : tx_Custom.Right - cbx_SystemVolume.Left;
		}

		private MountInfo CustomMountInfo()
		{
			if (!(tx_Custom.Visible && !string.IsNullOrEmpty(tx_Custom.Text)))
				return null;

			var mi = new MountInfo();
			string text = tx_Custom.Text;
			var M = Common.volumeRx.Match(text);
			if (M.Success)
			{
				mi.DriveIdType = EDriveIdType.GUID;
				mi.DriveGUID = @"\\?\" + M.Groups[1] + @"\";
				mi.DriveMountPoint = null;
				return mi;
			}
			else
			{
				M = Common.driveRx.Match(text);
				if (M.Success)
				{
					text = M.Groups[1].Value.ToUpperInvariant() + @"\";
				}
				mi.DriveIdType = EDriveIdType.MountPoint;
				mi.DriveMountPoint = text;
				mi.DriveGUID = null;
				return mi;
			}
		}

		bool _selected = false;
		private string m_ComputerName;
		private string m_MachineId;

		public void SetTabSelected(bool selected)
		{
			if (_selected == selected)
				return;
			_selected = selected;
			if (_selected)
			{
				if (PwEntryForm != null)
					PwEntryForm.UpdateEntryStrings(true, false);

				FillVolumes();
				SettingsLoad();
			}
			else
			{
				SettingsSave();
				if (PwEntryForm != null)
					PwEntryForm.UpdateEntryStrings(false, false);
			}
		}

		private void btn_Clear_Click(object sender, EventArgs e)
		{
			if (DialogResult.Yes != MessageBox.Show(btn_Clear, "Reset all " + KeeLocker.Globals.APP_NAME + " entry setting", "Clear settings", MessageBoxButtons.YesNo))
				return;

			m_EntryData = new EntryData();
			UpdateUi();
		}

		private void icon_Click(object sender, EventArgs e)
		{
			m_plugin.OpenHomepage();
		}

		private void tx_Custom_TextChanged(object sender, EventArgs e)
		{
			if (tx_Custom.Visible)
			{
				bool bAddEnabled = null != CustomMountInfo();
				btn_Add.Enabled = bAddEnabled;
				btn_AddMachine.Enabled = bAddEnabled;
			}
		}

		private void tx_Custom_Validated(object sender, EventArgs e)
		{
			if (!tx_Custom.Visible)
				return;
		}

		private void btn_Delete_Click(object sender, EventArgs e)
		{
			// TODO: ask confirmation
			var mounts = new List<MountInfo>();

			foreach (ListViewItem lvi in lvMounts.Items)
			{
				if (!lvi.Selected)
					mounts.Add((MountInfo)lvi.Tag);
			}
			m_EntryData.Mounts = mounts;
			UpdateUi();
		}

		private void btn_Add_Click(object sender, EventArgs e)
		{
			if (tx_Custom.Visible)
				tx_Custom_TextChanged(sender, EventArgs.Empty);
			var vi = (VolumeInfo)cbx_SystemVolume.SelectedData;
			if (vi == null)
				return;
			AddMountInfo(new MountInfo
			{
				DriveGUID = vi.Volume,
				DriveMountPoint = vi.MountPoint,
				DriveIdType = EDriveIdType.GUID
			}, false);
		}

		private void btn_AddMachine_Click(object sender, EventArgs e)
		{
			if (tx_Custom.Visible)
				tx_Custom_TextChanged(sender, EventArgs.Empty);
			var vi = (VolumeInfo)cbx_SystemVolume.SelectedData;
			if (vi == null)
				return;
			AddMountInfo(new MountInfo
			{
				DriveGUID = vi.Volume,
				DriveMountPoint = vi.MountPoint,
				DriveIdType = EDriveIdType.GUID
			}, true);
		}
		private void AddMountInfo(MountInfo mi, bool restrictMachine)
		{
			if (string.IsNullOrEmpty(mi.DriveGUID) && string.IsNullOrEmpty(mi.DriveMountPoint))
				return; // TODO set status

			if (restrictMachine)
			{
				mi.ComputerName = m_ComputerName;
				mi.MachineId = m_MachineId;
			}

			int z = m_EntryData.Mounts.FindIndex(m =>
			 m.DriveIdType == mi.DriveIdType &&
			 string.Equals(m.DriveGUID, mi.DriveGUID, StringComparison.InvariantCultureIgnoreCase) &&
			 string.Equals(m.DriveMountPoint, mi.DriveMountPoint, StringComparison.InvariantCultureIgnoreCase) &&
			 string.Equals(m.MachineId, mi.MachineId, StringComparison.InvariantCultureIgnoreCase) &&
			 string.Equals(m.ComputerName, mi.ComputerName, StringComparison.InvariantCultureIgnoreCase));

			if (z == -1)
			{
				z = m_EntryData.Mounts.Count;
				m_EntryData.Mounts.Add(mi);
			}
			m_EntryData.SelectedMount = z;
			UpdateUi();
		}

		private void lvMounts_SelectedIndexChanged(object sender, EventArgs e)
		{
			btn_Delete.Enabled = lvMounts.SelectedIndices.Count > 0;
		}

		private void btn_RefreshVolumes_Click(object sender, EventArgs e)
		{
			btn_RefreshVolumes.Enabled = false;
			RefreshVolumes();
			btn_RefreshVolumes.Enabled = true;
		}
	}
}