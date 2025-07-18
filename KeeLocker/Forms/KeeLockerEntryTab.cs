using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using KeePass.Forms;
using KeePass.Plugins;

namespace KeeLocker.Forms
{
	public partial class KeeLockerEntryTab : UserControl
	{
		private KeePass.Plugins.IPluginHost m_host;
		private KeeLockerExt m_plugin;
		private KeePassLib.PwEntry m_entry;
		private KeePassLib.Collections.ProtectedStringDictionary m_entrystrings;
		private readonly PwEntryForm PwEntryForm;

		// settings
		private EDriveIdType m_DriveIdType;
		private string m_DriveMountPoint;
		private string m_DriveGUID;
		private bool m_UnlockOnOpening;
		private bool m_UnlockOnConnection;
		private bool m_IsRecoveryKey;

		private Regex volumeRx = new Regex(@"^(?:\\{2}\?\\)?(Volume\{[0-9a-z-]+\})\\*", RegexOptions.IgnoreCase); // \\?\Volume{1c794602-2372-11ee-a970-b42e99f6c353}\
		private Regex driveRx = new Regex(@"^(?:\\{2}\?\\)?([a-z]:)\\*$", RegexOptions.IgnoreCase); // \\?\Volume{1c794602-2372-11ee-a970-b42e99f6c353}\



		public static EDriveIdType GetDriveIdTypeFromString(KeePassLib.Security.ProtectedString DriveIdType)
		{
			if (DriveIdType != null)
			{
				string DriveIdTypeString = DriveIdType.ReadString();
				if (DriveIdTypeString == EDriveIdType.MountPoint.ToString())
					return EDriveIdType.MountPoint;
				else if (DriveIdTypeString == EDriveIdType.GUID.ToString())
					return EDriveIdType.GUID;
			}
			return Common.DriveIdTypeDefault;
		}

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

			SetStatus(null);

			UpdateUi();
		}

		public static IList<VolumeInfo> EnumVolumeInfo()
		{
			int M = 1024;
			IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

			IList<VolumeInfo> volumeInfo = new List<VolumeInfo>();

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
								if (di.Name.Equals(vi.MountPoint, StringComparison.InvariantCultureIgnoreCase))
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

			{
				KeePassLib.Security.ProtectedString DriveIdType = m_entrystrings.Get(KeeLockerExt.StringName_DriveIdType);
				m_DriveIdType = GetDriveIdTypeFromString(DriveIdType);
			}

			{
				KeePassLib.Security.ProtectedString DriveMountPoint = m_entrystrings.Get(KeeLockerExt.StringName_DriveMountPoint);
				m_DriveMountPoint = DriveMountPoint != null ? DriveMountPoint.ReadString() : "";
			}

			{
				KeePassLib.Security.ProtectedString DriveGUID = m_entrystrings.Get(KeeLockerExt.StringName_DriveGUID);
				m_DriveGUID = DriveGUID != null ? DriveGUID.ReadString() : "";
			}

			{
				KeePassLib.Security.ProtectedString UnlockOnOpening = m_entrystrings.Get(KeeLockerExt.StringName_UnlockOnOpening);
				m_UnlockOnOpening = Common.GetBoolSetting(UnlockOnOpening, Common.DefaultUnlockOnOpening);
			}
			{
				KeePassLib.Security.ProtectedString UnlockOnConnection = m_entrystrings.Get(KeeLockerExt.StringName_UnlockOnConnection);
				m_UnlockOnConnection = Common.GetBoolSetting(UnlockOnConnection, Common.DefaultUnlockOnConnection);
			}
			{
				KeePassLib.Security.ProtectedString IsRecoveryKey = m_entrystrings.Get(KeeLockerExt.StringName_IsRecoveryKey);
				m_IsRecoveryKey = Common.GetBoolSetting(IsRecoveryKey, Common.DefaultIsRecoveryKey);
			}

			if (string.IsNullOrEmpty(m_DriveGUID) && string.IsNullOrEmpty(m_DriveMountPoint))
			{
				cbx_SystemVolume.SelectedIndex = 0;
				return;
			}
			bool found = false;
			foreach (object tmp in cbx_SystemVolume.Items)
			{
				VolumeInfo vi = (VolumeInfo)cbx_SystemVolume.GetDataForItem(tmp);
				if (vi != null && vi.Volume != null && m_DriveIdType == vi.DriveIdType)
				{

					if ((vi.DriveIdType == EDriveIdType.GUID && vi.Volume.Equals(m_DriveGUID, StringComparison.InvariantCultureIgnoreCase))
					|| (vi.DriveIdType == EDriveIdType.MountPoint && vi.MountPoint.Equals(m_DriveMountPoint, StringComparison.InvariantCultureIgnoreCase)))
					{
						cbx_SystemVolume.SelectedItem = tmp;
						found = true;
						break;
					}
				}
			}
			if (!found)
			{
				VolumeInfo vi = new VolumeInfo
				{
					DriveIdType = m_DriveIdType,
					Volume = m_DriveGUID,
					MountPoint = m_DriveMountPoint,
				};
				int z = cbx_SystemVolume.Item_Add(new RichComboBox.SItem("(Current) " + vi.DisplayText, RichComboBox.EItemType.Active, vi));
				cbx_SystemVolume.SelectedIndex = z;

			}
		}

		private void SettingsSave(string SettingName, string SettingValue)
		{
			if (string.IsNullOrEmpty(SettingValue))
			{
				m_entrystrings.Remove(SettingName);
			}
			else
			{
				KeePassLib.Security.ProtectedString PreviousValue = m_entrystrings.Get(SettingName);
				if (PreviousValue == null || SettingValue != PreviousValue.ReadString())
				{
					m_entrystrings.Set(SettingName, new KeePassLib.Security.ProtectedString(false, SettingValue));
				}
			}
		}

		private void SettingsSave()
		{
			if (string.IsNullOrEmpty(m_DriveMountPoint) && string.IsNullOrEmpty(m_DriveGUID))
			{
				// if neither mountpoint nor volume is set clear all KeeLocker values
				SettingsSave(KeeLockerExt.StringName_DriveIdType, null);
				SettingsSave(KeeLockerExt.StringName_DriveMountPoint, null);
				SettingsSave(KeeLockerExt.StringName_DriveGUID, null);
				SettingsSave(KeeLockerExt.StringName_UnlockOnOpening, null);
				SettingsSave(KeeLockerExt.StringName_UnlockOnConnection, null);
				SettingsSave(KeeLockerExt.StringName_IsRecoveryKey, null);
			}
			else
			{
				SettingsSave(KeeLockerExt.StringName_DriveIdType, m_DriveIdType == Common.DriveIdTypeDefault ? "" : m_DriveIdType.ToString());
				SettingsSave(KeeLockerExt.StringName_DriveMountPoint, m_DriveMountPoint);
				SettingsSave(KeeLockerExt.StringName_DriveGUID, m_DriveGUID);
				SettingsSave(KeeLockerExt.StringName_UnlockOnOpening, Common.BoolFor(m_UnlockOnOpening, Common.DefaultUnlockOnOpening));
				SettingsSave(KeeLockerExt.StringName_UnlockOnConnection, Common.BoolFor(m_UnlockOnConnection, Common.DefaultUnlockOnConnection));
				SettingsSave(KeeLockerExt.StringName_IsRecoveryKey, Common.BoolFor(m_IsRecoveryKey, Common.DefaultIsRecoveryKey));
			}
		}



		private void UpdateUi()
		{
			chk_UnlockOnOpening.Checked = m_UnlockOnOpening;
			chk_UnlockOnConnection.Checked = m_UnlockOnConnection;
			chk_IsRecoveryKey.Checked = m_IsRecoveryKey;

			VolumeInfo vi = (VolumeInfo)cbx_SystemVolume.GetDataForItem(cbx_SystemVolume.SelectedItem);
			if (vi != null && vi.DriveIdType == m_DriveIdType && string.Equals(vi.MountPoint, m_DriveMountPoint, StringComparison.InvariantCultureIgnoreCase) && string.Equals(vi.Volume, m_DriveGUID, StringComparison.InvariantCultureIgnoreCase))
			{
			}
			else if (cbx_SystemVolume.Items.Count > 0)
			{
				cbx_SystemVolume.SelectedIndexChanged -= cbx_SystemVolume_SelectedIndexChanged;
				cbx_SystemVolume.SelectedIndex = 0;
				cbx_SystemVolume.SelectedIndexChanged += cbx_SystemVolume_SelectedIndexChanged;
				vi = (VolumeInfo)cbx_SystemVolume.GetDataForItem(cbx_SystemVolume.SelectedItem);
			}
			bool bShowCustom = vi == null || vi.DriveIdType == EDriveIdType.MountPoint || string.IsNullOrEmpty(vi.Volume);
			tx_Custom.Visible = bShowCustom;
			lbl_Custom.Visible = bShowCustom;
			tx_Custom.Enabled = false;
			tx_Custom.Text = (bShowCustom && vi != null) ? vi.CustomText : "";
			tx_Custom.Enabled = bShowCustom;
		}

		public void OnSave(object sender, EventArgs e)
		{
			if (_selected) // if not selected it was never opened or is already saved because of tab switch
				SettingsSave();
		}

		private void rdo_MountPoint_Click(object sender, EventArgs e)
		{
			m_DriveIdType = EDriveIdType.MountPoint;
			UpdateUi();
			SetStatus(null);
		}

		private void rdo_DriveGUID_Click(object sender, EventArgs e)
		{
			m_DriveIdType = EDriveIdType.GUID;
			UpdateUi();
			SetStatus(null);
		}

		private void chk_UnlockOnOpening_Click(object sender, EventArgs e)
		{
			m_UnlockOnOpening = chk_UnlockOnOpening.Checked;
			UpdateUi();
			SetStatus(null);

		}
		private void chk_UnlockOnConnection_Click(object sender, EventArgs e)
		{
			m_UnlockOnConnection = chk_UnlockOnConnection.Checked;
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
			KeePassLib.Security.ProtectedString Password = Strings.Get(KeeLockerExt.StringName_Password);
			KeePassLib.Security.ProtectedString IsRecoveryKey = Strings.Get(KeeLockerExt.StringName_IsRecoveryKey);
			this.btn_Unlock.Enabled = false;
			SetStatus("Unlocking...");

			var item = new BitLockerItem(m_DriveIdType,
				  new KeePassLib.Security.ProtectedString(true, m_DriveMountPoint),
				  new KeePassLib.Security.ProtectedString(true, m_DriveGUID),
				  Password,
		  Common.GetBoolSetting(IsRecoveryKey, Common.DefaultIsRecoveryKey));

			Common.UnlockBitLocker(new List<BitLockerItem> { item }, EUnlockReason.UserRequest, this, (long SucceededCount, long AttemptedCount) =>
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
			// TODO
		}

		private void btn_DriveGUID_Click(object sender, EventArgs e)
		{
			SetStatus("Detect volume GUID...");
			string DriveGUID;
			bool Ok = FveApi.GetDriveGUID(m_DriveMountPoint, out DriveGUID);
			if (Ok)
			{
				m_DriveGUID = DriveGUID;
				m_DriveIdType = EDriveIdType.GUID;
				SetStatus(null);
			}
			else
			{
				m_DriveGUID = "";
				m_DriveIdType = EDriveIdType.MountPoint;
				SetStatus("Unable to get GUID", true);
			}
			UpdateUi();
		}

		private void chk_IsRecoveryKey_Click(object sender, EventArgs e)
		{
			m_IsRecoveryKey = chk_IsRecoveryKey.Checked;
			UpdateUi();
		}

		private void cbx_SystemVolume_SelectedIndexChanged(object sender, EventArgs e)
		{
			VolumeInfo vi = (VolumeInfo)cbx_SystemVolume.GetDataForItem(cbx_SystemVolume.SelectedItem);
			if (vi == null) return;

			m_DriveIdType = vi.DriveIdType;
			m_DriveGUID = vi.Volume;
			m_DriveMountPoint = vi.MountPoint;
			UpdateUi();
			SetStatus(null);
		}

		bool _selected = false;

		public void SetTabSelected(bool selected)
		{
			if (_selected == selected)
				return;
			_selected = selected;
			if (_selected)
			{
				if (PwEntryForm != null)
					PwEntryForm.UpdateEntryStrings(true, false);

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
			m_DriveIdType = Common.DriveIdTypeDefault;

			m_DriveGUID = "";
			m_DriveMountPoint = "";
			m_UnlockOnConnection = Common.DefaultUnlockOnConnection;
			m_UnlockOnOpening = Common.DefaultUnlockOnOpening;
			m_IsRecoveryKey = Common.DefaultIsRecoveryKey;
			UpdateUi();
		}

		private void icon_Click(object sender, EventArgs e)
		{
			m_plugin.OpenHomepage();
		}

		private void tx_Custom_TextChanged(object sender, EventArgs e)
		{
			VolumeInfo vi = (VolumeInfo)cbx_SystemVolume.GetDataForItem(cbx_SystemVolume.SelectedItem);

			if (vi != null && tx_Custom.Enabled)
			{
				string text = tx_Custom.Text;
				string custom = vi.CustomText;
				if (!text.Equals(custom, StringComparison.InvariantCultureIgnoreCase))
				{
					cbx_SystemVolume.SelectedIndexChanged -= cbx_SystemVolume_SelectedIndexChanged;
					cbx_SystemVolume.SelectedIndex = 0;
					cbx_SystemVolume.SelectedIndexChanged += cbx_SystemVolume_SelectedIndexChanged;
				}
			}



		}

		private void tx_Custom_Validated(object sender, EventArgs e)
		{
			if (!tx_Custom.Visible)
				return;

			string text = tx_Custom.Text;
			var M = volumeRx.Match(text);
			if (M.Success)
			{
				m_DriveGUID = @"\\?\"+M.Groups[1]+@"\";
				m_DriveMountPoint = null;
				m_DriveIdType = EDriveIdType.GUID;
			}
			else
			{
				M= driveRx.Match(text);
				if (M.Success)
				{
					text = M.Groups[1].Value.ToUpperInvariant() + @"\";
				}
				m_DriveGUID = null;
				m_DriveMountPoint = text;
				m_DriveIdType = EDriveIdType.MountPoint;
			}
		}
	}
}