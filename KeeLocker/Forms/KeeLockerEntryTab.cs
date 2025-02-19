﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text;
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
    private KeeLockerExt.EDriveIdType m_DriveIdType;
	private string m_DriveMountPoint;
	private string m_DriveGUID;
	private bool m_UnlockOnOpening;
	private bool m_UnlockOnConnection;
	private bool m_IsRecoveryKey;

	const KeeLockerExt.EDriveIdType DriveIdTypeDefault = KeeLockerExt.EDriveIdType.MountPoint;
	public const bool DefaultIsRecoveryKey = false;
	public const bool DefaultUnlockOnConnection = false;
	public const bool DefaultUnlockOnOpening = false;


	public static KeeLockerExt.EDriveIdType GetDriveIdTypeFromString(KeePassLib.Security.ProtectedString DriveIdType)
	{
	  if (DriveIdType != null)
	  {
		string DriveIdTypeString = DriveIdType.ReadString();
		if (DriveIdTypeString == KeeLockerExt.EDriveIdType.MountPoint.ToString())
		  return KeeLockerExt.EDriveIdType.MountPoint;
		else if (DriveIdTypeString == KeeLockerExt.EDriveIdType.GUID.ToString())
		  return KeeLockerExt.EDriveIdType.GUID;
	  }
	  return DriveIdTypeDefault;
	}

	public static bool GetBoolSetting(KeePassLib.Security.ProtectedString Value, bool defaultValue)
	{
	  if (Value == null)
		return defaultValue;
	  string tmp = Value.ReadString().Trim().ToLower();
	  switch (tmp)
	  {
		case "true":
		  return true;
		case "false":
		  return false;
		default:
		  return defaultValue;
	  }
	}
	private static string BoolFor(bool Value, bool defaultValue)
	{
	  if (Value == defaultValue)
		return "";

	  return Value ? "true" : "false";
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

	  SetStatus(null);

	  UpdateUi();
	}

	public class VolumeInfo
	{
	  public string MountPoint { get; set; }
	  public string Volume { get; set; }
	  public KeeLockerExt.EDriveIdType DriveIdType { get; set; }
	  public System.IO.DriveInfo DriveInfo { get; set; }
	  public string DisplayText
	  {
		get
		{
		  StringBuilder sb = new StringBuilder();
		  if (!string.IsNullOrWhiteSpace(MountPoint))
		  {
			string mp = MountPoint.Trim().TrimEnd('\\');
			sb.Append(mp);
		  }

		  if (DriveInfo != null)
		  {
			try
			{
			  if (!string.IsNullOrWhiteSpace(DriveInfo.VolumeLabel))
			  {
				if (sb.Length > 0) sb.Append(' ');
				sb.AppendFormat("[{0}]", DriveInfo.VolumeLabel.Trim());
			  }
			  if (DriveInfo.TotalSize > 0)
			  {
				if (sb.Length > 0) sb.Append(' ');
				sb.AppendFormat("{0}", format_size(DriveInfo.TotalSize));
			  }
			}
			catch (Exception) { /*may throw if drive is encrypted */ }


		  }

		  if (!string.IsNullOrWhiteSpace(Volume))
		  {
			string vol = Volume.Trim();
			if (vol.StartsWith("\\\\?\\") && vol.EndsWith("\\"))
			  vol = vol.Substring(4).TrimEnd('\\');
			if (sb.Length > 0) sb.Append(" - ");
			sb.Append(vol);
		  }

		  return sb.ToString();
		}
	  }
	}


	private static string format_size(long totalSize)
	{
	  if (totalSize <= 0)
		return "?";

	  const long D = 1024;
	  const string suffix = "  KBMBGBTB";
	  if (totalSize < 1024)
		return totalSize.ToString();
	  double f = totalSize;
	  int s = 0;
	  while (f >= D && s < 4)
	  {
		f /= D;
		s++;
	  }
	  if (f < 100)
	  {
		return f.ToString("F1") + suffix.Substring(s * 2, 2);
	  }
	  return Math.Floor(f).ToString("F0") + suffix.Substring(s * 2, 2); ;
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
		  Debug.WriteLine("{0} -> {1}", vshort, disk);
		  List<string> tmp = null;
		  bool added = false;
		  if (FveApi.GetVolumePathNamesForVolumeName(vol, out tmp) && tmp.Count > 0)
		  {
			Debug.WriteLine("   {0}", string.Join(",", tmp), false);
			foreach (string dl in tmp)
			{
			  added = true;
			  VolumeInfo vi = new VolumeInfo { MountPoint = dl, Volume = vol, DriveIdType = KeeLockerExt.EDriveIdType.GUID };
			  volumeInfo.Add(vi);
			}
		  }
		  if (!added)
		  {
			VolumeInfo vi = new VolumeInfo { MountPoint = null, Volume = vol, DriveIdType = KeeLockerExt.EDriveIdType.GUID };
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
				  DriveIdType = KeeLockerExt.EDriveIdType.MountPoint
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
	  this.cbx_DriveMountPoint.Items.Clear();
	  this.cbx_DriveGUID.Items.Clear();
	  VolumeInfos = EnumVolumeInfo();
	  this.cbx_SystemVolume.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("(None)", RichComboBox.EItemType.Active, new VolumeInfo
	  {
		MountPoint = "",
		Volume = "",
		DriveIdType = DriveIdTypeDefault
	  }));

	  SortedSet<string> mpd = new SortedSet<string>();
	  SortedSet<string> mpf = new SortedSet<string>();
	  SortedSet<string> vs = new SortedSet<string>();
	  SortedDictionary<string, VolumeInfo> sv = new SortedDictionary<string, VolumeInfo>();


	  foreach (VolumeInfo vi in this.VolumeInfos)
	  {
		sv.Add(vi.DisplayText, vi);
		if (!string.IsNullOrWhiteSpace(vi.MountPoint))
		{
		  if (vi.MountPoint.EndsWith(":\\") && vi.MountPoint.Length == 3)
			mpd.Add(vi.MountPoint);
		  else
			mpf.Add(vi.MountPoint);
		}
		if (!string.IsNullOrWhiteSpace(vi.Volume))
		  vs.Add(vi.Volume);
	  }
	  foreach (KeyValuePair<string, VolumeInfo> kv in sv)
	  {
		this.cbx_SystemVolume.Item_Add(new KeeLocker.Forms.RichComboBox.SItem(kv.Key, RichComboBox.EItemType.Active, kv.Value));
	  }
	  this.cbx_DriveGUID.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("Volumes:", RichComboBox.EItemType.Inactive));
	  foreach (string v in vs)
	  {
		this.cbx_DriveGUID.Item_Add(new KeeLocker.Forms.RichComboBox.SItem(v, RichComboBox.EItemType.Active));
	  }

	  this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("Can be a drive root like:", RichComboBox.EItemType.Inactive));
	  if (mpd.Count == 0) mpd.Add("D:\\");
	  foreach (string v in mpd)
	  {
		this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem(v, RichComboBox.EItemType.Active));
	  }
	  this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("Or any valid mountpoint path", RichComboBox.EItemType.Inactive));
	  this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("C:\\Path\\To\\MountPoint", RichComboBox.EItemType.Active));
	  if (mpf.Count == 0) mpf.Add("C:\\Path\\To\\MountPoint");

	  foreach (string v in mpf)
	  {
		this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem(v, RichComboBox.EItemType.Active));
	  }

	  this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("Or an exotic mountpoint", RichComboBox.EItemType.Inactive));
	  this.cbx_DriveMountPoint.Item_Add(new KeeLocker.Forms.RichComboBox.SItem("like a network drive, etc...", RichComboBox.EItemType.Inactive));
	  this.cbx_DriveMountPoint.MaxDropDownItems = this.cbx_DriveMountPoint.Items.Count;

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
		m_UnlockOnOpening = GetBoolSetting(UnlockOnOpening, DefaultUnlockOnOpening);
	  }
	  {
		KeePassLib.Security.ProtectedString UnlockOnConnection = m_entrystrings.Get(KeeLockerExt.StringName_UnlockOnConnection);
		m_UnlockOnConnection = GetBoolSetting(UnlockOnConnection, DefaultUnlockOnConnection);
	  }
	  {
		KeePassLib.Security.ProtectedString IsRecoveryKey = m_entrystrings.Get(KeeLockerExt.StringName_IsRecoveryKey);
		m_IsRecoveryKey = GetBoolSetting(IsRecoveryKey, DefaultIsRecoveryKey);
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

		  if ((vi.DriveIdType == KeeLockerExt.EDriveIdType.GUID && vi.Volume.Equals(m_DriveGUID, StringComparison.InvariantCultureIgnoreCase))
		  || (vi.DriveIdType == KeeLockerExt.EDriveIdType.MountPoint && vi.MountPoint.Equals(m_DriveMountPoint, StringComparison.InvariantCultureIgnoreCase)))
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
		int z = cbx_SystemVolume.Item_Add(new RichComboBox.SItem(vi.DisplayText, RichComboBox.EItemType.Active, vi));
		cbx_SystemVolume.SelectedIndex = z;

	  }


	  switch (m_DriveIdType)
	  {
		case KeeLockerExt.EDriveIdType.GUID:
		  if (!string.IsNullOrWhiteSpace(m_DriveGUID))
		  {

		  }
		  break;
		case KeeLockerExt.EDriveIdType.MountPoint:
		  if (!string.IsNullOrWhiteSpace(m_DriveMountPoint))
		  {

			foreach (object tmp in cbx_DriveMountPoint.Items)
			{
			  VolumeInfo vi = (VolumeInfo)cbx_DriveMountPoint.GetDataForItem(tmp);
			  if (vi != null && vi.MountPoint != null && vi.MountPoint.Equals(m_DriveMountPoint, StringComparison.InvariantCultureIgnoreCase))
			  {
				cbx_DriveMountPoint.SelectedItem = tmp;
				break;
			  }


			}
		  }
		  break;

		default:
		  cbx_SystemVolume.SelectedIndex = 0;
		  break;

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
	  SettingsSave(KeeLockerExt.StringName_DriveIdType, m_DriveIdType == DriveIdTypeDefault ? "" : m_DriveIdType.ToString());
	  SettingsSave(KeeLockerExt.StringName_DriveMountPoint, m_DriveMountPoint);
	  SettingsSave(KeeLockerExt.StringName_DriveGUID, m_DriveGUID);
	  SettingsSave(KeeLockerExt.StringName_UnlockOnOpening, BoolFor(m_UnlockOnOpening, DefaultUnlockOnOpening));
	  SettingsSave(KeeLockerExt.StringName_UnlockOnConnection, BoolFor(m_UnlockOnConnection, DefaultUnlockOnConnection));
	  SettingsSave(KeeLockerExt.StringName_IsRecoveryKey, BoolFor(m_IsRecoveryKey, DefaultIsRecoveryKey));
	}



	private void UpdateUi()
	{
	  cbx_DriveMountPoint.Text = m_DriveMountPoint;
	  cbx_DriveGUID.Text = m_DriveGUID;

	  rdo_MountPoint.Checked = m_DriveIdType == KeeLockerExt.EDriveIdType.MountPoint;
	  rdo_DriveGUID.Checked = m_DriveIdType == KeeLockerExt.EDriveIdType.GUID;

	  cbx_DriveMountPoint.Enabled = rdo_MountPoint.Checked;
	  btn_DriveGUID.Enabled = rdo_MountPoint.Checked;
	  cbx_DriveGUID.Enabled = rdo_DriveGUID.Checked;

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
	  }
	}

	public void OnSave(object sender, EventArgs e)
	{
	  if (_selected) // if not selected it was never opened or is already saved because of tab switch
		SettingsSave();
	}

	private void cbx_DriveMountPoint_Validated(object sender, EventArgs e)
	{
	  m_DriveMountPoint = cbx_DriveMountPoint.Text;
	  UpdateUi();
	}

	private void txt_DriveGUID_Validated(object sender, EventArgs e)
	{
	  m_DriveGUID = cbx_DriveGUID.Text;
	  UpdateUi();
	}

	private void rdo_MountPoint_Click(object sender, EventArgs e)
	{
	  m_DriveIdType = KeeLockerExt.EDriveIdType.MountPoint;
	  UpdateUi();
	  SetStatus(null);
	}

	private void rdo_DriveGUID_Click(object sender, EventArgs e)
	{
	  m_DriveIdType = KeeLockerExt.EDriveIdType.GUID;
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

	  var item = new KeeLockerExt.BitLockerItem(m_DriveIdType,
		new KeePassLib.Security.ProtectedString(true, m_DriveMountPoint),
		new KeePassLib.Security.ProtectedString(true, m_DriveGUID),
		Password,
		GetBoolSetting(IsRecoveryKey, DefaultIsRecoveryKey));

	  m_plugin.UnlockBitLocker(new List<KeeLockerExt.BitLockerItem> { item }, KeeLockerExt.EUnlockReason.UserRequest, this, (bool success) =>
	  {
		this.btn_Unlock.Enabled = true;
		if (success) SetStatus("Successfully unlocked");
		else SetStatus("Failed to unlock!", true);
	  });
	}

	private void btn_DriveGUID_Click(object sender, EventArgs e)
	{
	  SetStatus("Detect volume guid...");
	  string DriveGUID;
	  bool Ok = FveApi.GetDriveGUID(m_DriveMountPoint, out DriveGUID);
	  if (Ok)
	  {
		m_DriveGUID = DriveGUID;
		m_DriveIdType = KeeLockerExt.EDriveIdType.GUID;
		SetStatus(null);
	  }
	  else
	  {
		m_DriveGUID = "";
		m_DriveIdType = KeeLockerExt.EDriveIdType.MountPoint;
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
	  if (DialogResult.Yes != MessageBox.Show(btn_Clear,"Reset all KeeLocker entry setting", "Clear settings", MessageBoxButtons.YesNo))
		return;
	  m_DriveIdType = DriveIdTypeDefault;

	  m_DriveGUID = "";
	  m_DriveMountPoint = "";
	  m_UnlockOnConnection = DefaultUnlockOnConnection;
	  m_UnlockOnOpening = DefaultUnlockOnOpening;
	  m_IsRecoveryKey = DefaultIsRecoveryKey;
	  UpdateUi();
	}
  }
}
