using System.Collections.Generic;

namespace KeeLocker
{

	public class EntryData
	{
		public bool UnlockOnOpening { get; set; }
		public bool UnlockOnConnection { get; set; }

		public bool PasswordIsRecoveryKey { get; set; }
		public IDictionary<int, MountInfo> Mounts { get; set; }
		public int SelectedMount { get; set; }
		public ISet<string> ConfigKeys { get; set; }


		public EntryData()
		{
			UnlockOnOpening = Common.DefaultUnlockOnOpening;
			UnlockOnConnection = Common.DefaultUnlockOnConnection;
			PasswordIsRecoveryKey = Common.DefaultIsRecoveryKey;
			Mounts = new SortedDictionary<int, MountInfo>();
			SelectedMount = -1;
			ConfigKeys = new HashSet<string>();
		}
	}

	public class MountInfo 
	{
		public EDriveIdType DriveIdType { get; set; }
		public string DriveGUID { get; set; }
		public string DriveMountPoint { get; set; }
		public string MachineId { get; set; }
		public string ComputerName { get; set; }
	}

}

