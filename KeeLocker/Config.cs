using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace KeeLocker
{
	public static class Config
	{
		public static EntryData LoadKeelockerStringConfig(KeePassLib.Collections.ProtectedStringDictionary strings)
		{
			EntryData entry = new EntryData() { Version = 0 };

			EDriveIdType driveIdType;
			string mountPoint;
			string guid;
			bool hasStrValue = false;

			{
				KeePassLib.Security.ProtectedString DriveIdType = strings.Get(KeeLockerExt.StringName_DriveIdType);
				if (DriveIdType != null) hasStrValue = true;
				driveIdType =Common.GetDriveIdTypeFromString(DriveIdType);
			}

			{
				KeePassLib.Security.ProtectedString DriveMountPoint = strings.Get(KeeLockerExt.StringName_DriveMountPoint);
				if (DriveMountPoint != null) hasStrValue = true;
				mountPoint = DriveMountPoint != null ? DriveMountPoint.ReadString() : "";
			}

			{
				KeePassLib.Security.ProtectedString DriveGUID = strings.Get(KeeLockerExt.StringName_DriveGUID);
				if (DriveGUID != null) hasStrValue = true;
				guid = DriveGUID != null ? DriveGUID.ReadString() : "";
			}
			if (!string.IsNullOrEmpty(mountPoint) || !string.IsNullOrEmpty(guid))
			{
				entry.SelectedMount = entry.Mounts.Count;
				entry.Mounts.Add(new MountInfo()
				{
					DriveIdType = driveIdType,
					DriveGUID = guid,
					DriveMountPoint = mountPoint
				});
			}

			{
				KeePassLib.Security.ProtectedString UnlockOnOpening = strings.Get(KeeLockerExt.StringName_UnlockOnOpening);
				if (UnlockOnOpening != null) hasStrValue = true;
				entry.UnlockOnOpening = Config.GetBoolSetting(UnlockOnOpening, Common.DefaultUnlockOnOpening);
			}
			{
				KeePassLib.Security.ProtectedString UnlockOnConnection = strings.Get(KeeLockerExt.StringName_UnlockOnConnection);
				if (UnlockOnConnection != null) hasStrValue = true;
				entry.UnlockOnConnection = Config.GetBoolSetting(UnlockOnConnection, Common.DefaultUnlockOnConnection);
			}
			{
				KeePassLib.Security.ProtectedString IsRecoveryKey = strings.Get(KeeLockerExt.StringName_IsRecoveryKey);
				if (IsRecoveryKey != null) hasStrValue = true;
				entry.PasswordIsRecoveryKey = Config.GetBoolSetting(IsRecoveryKey, Common.DefaultIsRecoveryKey);
			}
			return hasStrValue ? entry : null;
		}

		public static EntryData LoadEntryData(KeePassLib.Collections.ProtectedStringDictionary strings)
		{
			KeePassLib.Security.ProtectedString V1 = strings.Get(KeeLockerExt.StringName_V1);
			if (V1 == null || V1.IsEmpty)
				return null;
			string xml = V1.ReadString();
			if (string.IsNullOrEmpty(xml))
				return null;

			XmlSerializer serializer = new XmlSerializer(typeof(EntryData));
			using (StringReader reader = new StringReader(xml))
			{
				EntryData entry = (EntryData)serializer.Deserialize(reader);
				if (entry.Mounts == null) entry.Mounts = new List<MountInfo>();
				return entry;
			}
		}

		public static void SaveEntryData(KeePassLib.Collections.ProtectedStringDictionary strings, EntryData entryData)
		{
			if (entryData.Equals(new EntryData() { Version = entryData.Version, SelectedMount = entryData.SelectedMount }))
			{
				SetStringValue(strings, KeeLockerExt.StringName_V1, null);
			}
			else
			{
				entryData.Version = (int)Versions.Current;

				XmlSerializer serializer = new XmlSerializer(entryData.GetType());
				StringWriter writer = new StringWriter();
				serializer.Serialize(writer, entryData);
				SetStringValue(strings, KeeLockerExt.StringName_V1, writer.ToString(), true);
			}
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
