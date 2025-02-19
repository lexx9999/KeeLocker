using KeePassLib.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace KeeLocker
{

	public static class Common
	{
		public const EDriveIdType DriveIdTypeDefault = EDriveIdType.MountPoint;
		public const bool DefaultIsRecoveryKey = false;
		public const bool DefaultUnlockOnConnection = false;
		public const bool DefaultUnlockOnOpening = false;
		internal static string FormatSize(long totalSize)
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

		internal static bool GetBoolSetting(KeePassLib.Security.ProtectedString Value, bool defaultValue)
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
		internal static string BoolFor(bool Value, bool defaultValue)
		{
			if (Value == defaultValue)
				return "";

			return Value ? "true" : "false";
		}

		private static void TryUnlockVolume_Thread(IEnumerable<BitLockerItem> bitLockerItems, EUnlockReason UnlockReason, System.Windows.Forms.Control target, UnlockResultDelegate unlockResult)
		{
			// filter attempts by results of FindFirstVolumeW ... and QueryDosDeviceW
			// https://learn.microsoft.com/en-us/windows/win32/fileio/displaying-volume-paths

			var VolumeInfos = KeeLocker.Forms.KeeLockerEntryTab.EnumVolumeInfo();


			bool success = true;
			foreach (BitLockerItem item in bitLockerItems)
			{
				if (!item.MaybeThisSystem(VolumeInfos))
					continue;
				try
				{
					FveApi.Result result = item.Unlock();
					if (result != FveApi.Result.Ok)
						success = false;

				}
				catch (Exception Ex)
				{
					string Messages = Ex.ToString();
					success = false;
				}
			}
			if (target != null && target.InvokeRequired) target.Invoke(new UnlockResultDelegate(unlockResult), new object[] { success });
			else if (unlockResult != null) unlockResult(success);
		}

		public delegate void UnlockResultDelegate(bool Success);
		internal static void UnlockBitLocker(IEnumerable<BitLockerItem> bitLockerItems, EUnlockReason UnlockReason, System.Windows.Forms.Control target = null, UnlockResultDelegate unlockResult = null)
		{
			Thread thread = new Thread(() => TryUnlockVolume_Thread(bitLockerItems, UnlockReason, target, unlockResult));
			thread.Start();
		}

	}

	public enum EDriveIdType
	{
		MountPoint,
		GUID
	}

	public enum EUnlockReason
	{
		DatabaseOpening,
		DriveConnected,
		UserRequest
	};

	public class VolumeInfo
	{
		public string MountPoint { get; set; }
		public string Volume { get; set; }
		public EDriveIdType DriveIdType { get; set; }
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
							sb.AppendFormat("{0}", Common.FormatSize(DriveInfo.TotalSize));
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


	internal class BitLockerItem
	{
		private readonly EDriveIdType DriveIdType;
		private readonly KeePassLib.Security.ProtectedString DriveMountPoint;
		private readonly KeePassLib.Security.ProtectedString DriveGUID;
		private readonly KeePassLib.Security.ProtectedString Password;
		private readonly bool IsRecoveryKey;

		internal BitLockerItem(EDriveIdType driveIdType, ProtectedString driveMountPoint, ProtectedString driveGUID, ProtectedString password, bool isRecoveryKey)
		{
			DriveIdType = driveIdType;
			DriveMountPoint = driveMountPoint;
			DriveGUID = driveGUID;
			Password = password;
			IsRecoveryKey = isRecoveryKey;
		}

		internal bool MaybeThisSystem(IEnumerable<VolumeInfo> volumeInfos)
		{
			foreach (VolumeInfo vi in volumeInfos)
			{
				if (DriveIdType != vi.DriveIdType)
					continue;

				switch (DriveIdType)
				{
					case EDriveIdType.MountPoint:
						if (!string.IsNullOrEmpty(vi.MountPoint) && DriveMountPoint != null && !DriveMountPoint.IsEmpty &&
						string.Equals(vi.MountPoint, DriveMountPoint.ReadString(), StringComparison.InvariantCultureIgnoreCase))
							return true;
						break;
					case EDriveIdType.GUID:
						if (!string.IsNullOrEmpty(vi.Volume) && DriveGUID != null && !DriveGUID.IsEmpty &&
						  string.Equals(vi.Volume, DriveGUID.ReadString(), StringComparison.InvariantCultureIgnoreCase))
							return true;
						break;
				}
			}
			return false;
		}

		internal FveApi.Result Unlock()
		{
			string driveMountPoint = (DriveIdType == EDriveIdType.MountPoint && DriveMountPoint != null) ? DriveMountPoint.ReadString() : "";
			string driveGUID = (DriveIdType == EDriveIdType.GUID && DriveGUID != null) ? DriveGUID.ReadString() : "";

			if (driveGUID.Length > 0)
			{
				driveMountPoint = "";
			}
			else if (driveMountPoint.Length > 0)
			{
				driveGUID = "";
			}
			else
			{
				return FveApi.Result.WrongPassPhrase;
			}

			return FveApi.UnlockVolume(driveMountPoint, driveGUID, Password.ReadString(), IsRecoveryKey);
		}
	}
}