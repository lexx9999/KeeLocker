using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace KeeLocker
{
	public enum Versions
	{
		LegacyMigration = 0,
		V1 = 1,
		Current = V1
	};

	public class EntryData : IEquatable<EntryData>
	{
		[XmlElement("Version")]
		public int Version { get; set; }
		[XmlElement("UnlockOnOpening")]
		public bool UnlockOnOpening { get; set; }
		[XmlElement("UnlockOnConnection")]
		public bool UnlockOnConnection { get; set; }

		[XmlElement("IsRecoveryKey")]
		public bool PasswordIsRecoveryKey { get; set; }

		[XmlArray("MountReferences")]
		[XmlArrayItem("MountInfo")]

		public List<MountInfo> Mounts { get; set; }

		[XmlIgnore]
		public int SelectedMount  { get; set; }


		public EntryData()
		{
			UnlockOnOpening = Common.DefaultUnlockOnOpening;
			UnlockOnConnection = Common.DefaultUnlockOnConnection;
			PasswordIsRecoveryKey = Common.DefaultIsRecoveryKey;
			Mounts = new List<MountInfo>();
			Version = 1;
			SelectedMount = -1;
		}

		public bool Equals(EntryData other)
		{
			if (ReferenceEquals(this, other)) return true;
			if (other == null) return false;

			if (Version != other.Version) return false;
			if (UnlockOnOpening != other.UnlockOnOpening) return false;
			if (UnlockOnConnection != other.UnlockOnConnection) return false;
			if (PasswordIsRecoveryKey != other.PasswordIsRecoveryKey) return false;

			// Compare Mounts list
			if (Mounts == null && other.Mounts != null) return false;
			if (Mounts != null && other.Mounts == null) return false;

			if (Mounts != null)
			{
				if (Mounts.Count != other.Mounts.Count) return false;

				for (int i = 0; i < Mounts.Count; i++)
				{
					if (!Equals(Mounts[i], other.Mounts[i]))
						return false;
				}
			}

			// SelectedMount intentionally ignored
			return true;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as EntryData);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 23 + Version.GetHashCode();
				hash = hash * 23 + UnlockOnOpening.GetHashCode();
				hash = hash * 23 + UnlockOnConnection.GetHashCode();
				hash = hash * 23 + PasswordIsRecoveryKey.GetHashCode();

				if (Mounts != null)
				{
					foreach (var m in Mounts)
						hash = hash * 23 + ((m==null) ? 0 : m.GetHashCode());
				}

				return hash;
			}
		}
	}

	public class MountInfo : IEquatable<MountInfo>
	{
		public EDriveIdType DriveIdType { get; set; }
		public string DriveGUID { get; set; }
		public string DriveMountPoint { get; set; }
		public string MachineId { get; set; }
		public string ComputerName { get; set; }

		public bool Equals(MountInfo other, StringComparison comparisonType)
		{
			if (ReferenceEquals(this, other)) return true;
			if (other == null) return false;

			return DriveIdType == other.DriveIdType
				&& string.Equals(DriveGUID, other.DriveGUID, comparisonType)
				&& string.Equals(DriveMountPoint, other.DriveMountPoint, comparisonType)
				&& string.Equals(MachineId, other.MachineId, comparisonType)
				&& string.Equals(ComputerName, other.ComputerName, comparisonType);
		}

		public bool Equals(MountInfo other)
		{
			if (ReferenceEquals(this, other)) return true;
			if (other == null) return false;

			return DriveIdType == other.DriveIdType
				&& string.Equals(DriveGUID, other.DriveGUID, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(DriveMountPoint, other.DriveMountPoint, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(MachineId, other.MachineId, StringComparison.OrdinalIgnoreCase)
				&& string.Equals(ComputerName, other.ComputerName, StringComparison.OrdinalIgnoreCase);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as MountInfo);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 17;

				hash = hash * 23 + DriveIdType.GetHashCode();
				hash = hash * 23 + (DriveGUID ?? "").GetHashCode();
				hash = hash * 23 + (DriveMountPoint ?? "").GetHashCode();
				hash = hash * 23 + (MachineId ?? "").GetHashCode();
				hash = hash * 23 + (ComputerName ?? "").GetHashCode();

				return hash;
			}
		}

		public static bool operator ==(MountInfo a, MountInfo b)
		{
			if (ReferenceEquals(a, b)) return true;
			if ((object)a == null || (object)b == null) return false;
			return a.Equals(b);
		}

		public static bool operator !=(MountInfo a, MountInfo b)
		{
			return !(a == b);
		}
	}

}

