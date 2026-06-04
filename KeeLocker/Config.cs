using KeePassLib.Collections;
using KeePassLib.Security;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;

namespace KeeLocker
{
	public static class Config
	{
		public static EntryData LoadKeelockerConfig(KeePassLib.Collections.ProtectedStringDictionary strings)
		{
			KeePassLib.Security.ProtectedString val;
			EntryData entry = new EntryData();
			if(TryGetSetting(strings, entry, Globals.CONFIG_PREFIX + KeeLockerExt.CfgUnlockOnOpening,out val)){
				entry.UnlockOnOpening = Config.GetBoolSetting(val, Common.DefaultUnlockOnOpening);
			}
			if (TryGetSetting(strings, entry, Globals.CONFIG_PREFIX + KeeLockerExt.CfgUnlockOnConnection,out val)){
				entry.UnlockOnConnection = Config.GetBoolSetting(val, Common.DefaultUnlockOnConnection);
			}
			if (TryGetSetting(strings, entry, Globals.CONFIG_PREFIX +KeeLockerExt.CfgIsRecoveryKey, out val))
			{
				entry.PasswordIsRecoveryKey = Config.GetBoolSetting(val, Common.DefaultIsRecoveryKey);
			}
			foreach (var kv in strings)
			{
				string key = kv.Key;
				string name;
				if (string.IsNullOrEmpty(key) || !key.StartsWith(Globals.CONFIG_PREFIX)) continue;
				int z = key.IndexOf('.');
                int m = 0;
				if (z != -1)
				{
					int w = key.IndexOf('.', z + 1);
					if (w == -1) // prefix<var>.<n>
					{
						if (!int.TryParse(key.Substring(z + 1), System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out m))
						{
							continue;
						}
						name = key.Substring(Globals.CONFIG_PREFIX.Length, z - Globals.CONFIG_PREFIX.Length);
					}
					else
					{ // prefix.<n>.<var>
						if (!int.TryParse(key.Substring(z + 1, w - z - 1), System.Globalization.NumberStyles.None, NumberFormatInfo.InvariantInfo, out m))
						{
							continue;
						}
						name = key.Substring(w + 1);
					}
				}
				else
				{
					name = key.Substring(Globals.CONFIG_PREFIX.Length);
				}
					switch (name)
					{
						case KeeLockerExt.CfgDriveIdType:
							{
								var mount = mountFor(entry.Mounts, m);
								mount.DriveIdType = Common.GetDriveIdTypeFromString(kv.Value);
								entry.ConfigKeys.Add(kv.Key);
							}
							break;

						case KeeLockerExt.CfgDriveMountPoint:
							{
								var mount = mountFor(entry.Mounts, m);
								mount.DriveMountPoint = (kv.Value == null) ? null : kv.Value.ReadString();
								entry.ConfigKeys.Add(kv.Key);
							}
							break;

						case KeeLockerExt.CfgDriveGUID:
							{
								var mount = mountFor(entry.Mounts, m);
								mount.DriveGUID = (kv.Value == null) ? null : kv.Value.ReadString();
								entry.ConfigKeys.Add(kv.Key);
							}
							break;
						case KeeLockerExt.CfgMachineId:
							{
								var mount = mountFor(entry.Mounts, m);
								mount.MachineId = (kv.Value == null) ? null : kv.Value.ReadString();
								entry.ConfigKeys.Add(kv.Key);
							}
							break;
						case KeeLockerExt.CfgComputerName:
							{
								var mount = mountFor(entry.Mounts, m);
								mount.ComputerName= (kv.Value == null) ? null : kv.Value.ReadString();
								entry.ConfigKeys.Add(kv.Key);
							}
							break;
					}
			}

			return (entry.ConfigKeys.Count > 0) ? entry : null;
		}

		private static bool TryGetSetting(ProtectedStringDictionary strings, EntryData entry, string name, out ProtectedString val)
		{
			val = strings.Get(name);
			if (val != null) entry.ConfigKeys.Add(name);
			return true;
		}

		private static MountInfo mountFor(IDictionary<int, MountInfo> mounts, int id)
		{
			MountInfo mount;
			if (mounts.TryGetValue(id, out mount)) return mount;
			mount = new MountInfo();
			mounts[id] = mount;
			return mount;
		}

		private class SettingsSaveHelper
		{
			private readonly ProtectedStringDictionary strings;

			ISet<string> deleteKeys = new HashSet<string>();
			ISet<string> newKeys = new HashSet<string>();
			public SettingsSaveHelper(KeePassLib.Collections.ProtectedStringDictionary strings)
			{
				this.strings = strings;
			}
			public void Set(string name, string value, bool protect = false)
			{
				if (string.IsNullOrEmpty(value))
				{
					strings.Remove(name);
					deleteKeys.Add(name);
				}
				else
				{
					KeePassLib.Security.ProtectedString prev = strings.Get(name);
					if (prev == null || value != prev.ReadString())
					{
						strings.Set(name, new KeePassLib.Security.ProtectedString(protect, value));
					}
					newKeys.Add(name);
				}
			}

			public ISet<string> Cleanup(ISet<string> configKeys)
			{
				var n = new HashSet<string>(configKeys);
				n.ExceptWith(deleteKeys);
				n.ExceptWith(newKeys);
				foreach (var key in n)
					strings.Remove(key);
				return newKeys;
			}
		}


		public static void SaveKeelockerConfig(KeePassLib.Collections.ProtectedStringDictionary strings, EntryData entry)
		{
			var ssh = new SettingsSaveHelper(strings);
			ssh.Set(Globals.CONFIG_PREFIX+ KeeLockerExt.CfgUnlockOnOpening, BoolFor(entry.UnlockOnOpening, Common.DefaultUnlockOnOpening));
			ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgUnlockOnConnection, BoolFor(entry.UnlockOnConnection, Common.DefaultUnlockOnOpening));
			ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgIsRecoveryKey, BoolFor(entry.PasswordIsRecoveryKey, Common.DefaultIsRecoveryKey));
			if (entry.Mounts.Count == 1 && entry.Mounts.ContainsKey(0))
			{
				// legacy mode (1.8 and earlier), with the extension of ComputerName/MachineId
				var m = entry.Mounts[0];
				ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgDriveIdType, m.DriveIdType == Common.DriveIdTypeDefault ? null : m.DriveIdType.ToString());
				ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgDriveMountPoint, m.DriveMountPoint);
				ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgDriveGUID, m.DriveGUID);
				ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgComputerName, m.ComputerName);
				ssh.Set(Globals.CONFIG_PREFIX + KeeLockerExt.CfgMachineId, m.MachineId);
			}
			else
			{

				foreach (var kv in entry.Mounts)
				{
					var prefix = string.Format("{0}.{1:D}.", Globals.CONFIG_PREFIX, kv.Key);
					var m = kv.Value;
					ssh.Set(prefix + KeeLockerExt.CfgDriveIdType, m.DriveIdType == Common.DriveIdTypeDefault ? null : m.DriveIdType.ToString());
					ssh.Set(prefix + KeeLockerExt.CfgDriveMountPoint, m.DriveMountPoint);
					ssh.Set(prefix + KeeLockerExt.CfgDriveGUID, m.DriveGUID);
					ssh.Set(prefix + KeeLockerExt.CfgComputerName, m.ComputerName);
					ssh.Set(prefix + KeeLockerExt.CfgMachineId, m.MachineId);
				}
			}
			entry.ConfigKeys = ssh.Cleanup(entry.ConfigKeys);
		}

		private static string vn(string name, int m)
		{
			if (name.StartsWith(Globals.CONFIG_PREFIX))
			{
				return string.Format("{0}.{1:D2}.{2}", Globals.CONFIG_PREFIX, m, name.Substring(Globals.CONFIG_PREFIX.Length));
			}
			// we should never get here
			return string.Format("{0}.{1}", name, m);
		}

		public static void SetStringValue(KeePassLib.Collections.ProtectedStringDictionary strings, string SettingName, string SettingValue, bool ProtectValue = false)
		{
			if (string.IsNullOrEmpty(SettingValue))
			{
				strings.Remove(SettingName);
			}
			else
			{
				KeePassLib.Security.ProtectedString PreviousValue = strings.Get(SettingName);
				if (PreviousValue == null || SettingValue != PreviousValue.ReadString())
				{
					strings.Set(SettingName, new KeePassLib.Security.ProtectedString(ProtectValue, SettingValue));
				}
			}
		}
		public static string BoolFor(bool Value, bool defaultValue)
		{
			if (Value == defaultValue)
				return "";

			return Value ? "true" : "false";
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
	}
}
