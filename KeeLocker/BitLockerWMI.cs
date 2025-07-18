using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Principal;
using System.Xml.Serialization;

namespace KeeLocker.BitLockerWMI
{
	public enum ProtectorType : uint
	{
		Unknown = 0,                          // Unknown or other protector type
		TrustedPlatformModule = 1,           // Trusted Platform Module (TPM)
		ExternalKey = 2,                     // External key
		NumericalPassword = 3,              // Numerical password
		TpmAndPin = 4,                       // TPM And PIN
		TpmAndStartupKey = 5,                // TPM And Startup Key
		TpmAndPinAndStartupKey = 6,          // TPM And PIN And Startup Key
		PublicKey = 7,                       // Public Key
		Passphrase = 8,                      // Passphrase
		TpmCertificate = 9,                  // TPM Certificate
		CngProtector = 10,                    // CryptoAPI Next Generation (CNG) Protector
	
		Unsupported = ~0u
	}

	public class KeyProtectorInfo
	{
		public string ID { get; set; }
		public ProtectorType Type { get; set; }

		public string NumericalPassword { get; set; }
	}
	public class VolumeInfo
	{
		public string DriveLetter { get; set; }
		public string VolumeID { get; set; }
		public string PersistentVolumeID { get; set; }
		public uint VolumeType { get; set; }
		public uint ProtectionStatus { get; set; }
		public uint EncryptionMethod { get; set; }
		public uint ConversionStatus { get; set; }
		[XmlArray("KeyProtectors")]
		[XmlArrayItem("KeyProtector")]

		public List<KeyProtectorInfo> KeyProtectors { get; set; }
	}

	internal static class BitLocker
	{

		public static bool IsAdministrator()
		{
			try
			{
				WindowsIdentity identity = WindowsIdentity.GetCurrent();
				WindowsPrincipal principal = new WindowsPrincipal(identity);
				return principal.IsInRole(WindowsBuiltInRole.Administrator);
			}
			catch
			{
				return false;
			}
		}

		public static List<VolumeInfo> GetBitLockerVolumes()
		{
			List<VolumeInfo> volumes = new List<VolumeInfo>();

			try
			{
				ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2\Security\MicrosoftVolumeEncryption");
				scope.Connect();

				ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_EncryptableVolume");
				ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

				foreach (ManagementObject volume in searcher.Get())
				{
					VolumeInfo info = new VolumeInfo();

					info.DriveLetter = volume["DriveLetter"] != null ? volume["DriveLetter"].ToString() : "";
					info.VolumeID = volume["DeviceID"] != null ? volume["DeviceID"].ToString() : "";
					info.PersistentVolumeID = volume["PersistentVolumeID"] != null ? volume["PersistentVolumeID"].ToString() : "";
					info.VolumeType = volume["VolumeType"] != null ? Convert.ToUInt32(volume["VolumeType"]) : 0;

					info.ProtectionStatus = GetUInt32Property(volume, "GetProtectionStatus", "ProtectionStatus");
					info.EncryptionMethod = GetUInt32Property(volume, "GetEncryptionMethod", "EncryptionMethod");
					info.ConversionStatus = GetUInt32Property(volume, "GetConversionStatus", "ConversionStatus");

					info.KeyProtectors = new List<KeyProtectorInfo>();
					const uint S_OK = 0;

					try
					{
						string[] ids;

						uint hr = GetKeyProtectors(volume, 0, out ids);
						if (hr != S_OK) ids = new string[0];

						foreach (string protectorId in ids)
						{
							ProtectorType KeyProtectorType;
							string NumericalPassword = null;
							hr = GetKeyProtectorType(volume, protectorId, out KeyProtectorType);
							if (hr != S_OK)
							{
								KeyProtectorType = ProtectorType.Unsupported;
							}

							if (KeyProtectorType == ProtectorType.NumericalPassword) // recoverykey
							{
								hr = GetKeyProtectorNumericalPassword(volume, protectorId, out NumericalPassword);
								if (hr != S_OK)
								{
									NumericalPassword = null;
								}
							}

							info.KeyProtectors.Add(new KeyProtectorInfo
							{
								ID =  protectorId,
								Type = KeyProtectorType,
								NumericalPassword =  NumericalPassword,
							});
						}

					}
					catch (Exception ex)
					{
						// Optional: log or include error info
						Console.WriteLine(ex.ToString());
					}
					if (info.KeyProtectors.Count == 0)
						info.KeyProtectors = null;


					volumes.Add(info);
				}
			}
			catch { }

			return volumes;
		}

		private static uint GetKeyProtectors(ManagementObject volume, uint KeyProtectorType, out string[] VolumeKeyProtectorID)
		{
			System.Management.ManagementBaseObject inParams = null;
			inParams = volume.GetMethodParameters("GetKeyProtectors");
			inParams["KeyProtectorType"] = ((uint)(KeyProtectorType));
			System.Management.ManagementBaseObject outParams = volume.InvokeMethod("GetKeyProtectors", inParams, null);
			VolumeKeyProtectorID = ((string[])(outParams.Properties["VolumeKeyProtectorID"].Value));
			return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
		}
		private static uint GetKeyProtectorType(ManagementObject volume, string VolumeKeyProtectorID, out ProtectorType KeyProtectorType)
		{
			System.Management.ManagementBaseObject inParams = null;
			inParams = volume.GetMethodParameters("GetKeyProtectorType");
			inParams["VolumeKeyProtectorID"] = ((string)(VolumeKeyProtectorID));
			System.Management.ManagementBaseObject outParams = volume.InvokeMethod("GetKeyProtectorType", inParams, null);
			uint val = System.Convert.ToUInt32(outParams.Properties["KeyProtectorType"].Value);
			if (Enum.IsDefined(typeof(ProtectorType), val))
			{
				KeyProtectorType = (ProtectorType)val;
			}
			else
				KeyProtectorType = ProtectorType.Unsupported;

			return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
		}

		private static uint GetKeyProtectorNumericalPassword(ManagementObject volume, string VolumeKeyProtectorID, out string NumericalPassword)
		{
			System.Management.ManagementBaseObject inParams = null;
			inParams = volume.GetMethodParameters("GetKeyProtectorNumericalPassword");
			inParams["VolumeKeyProtectorID"] = ((string)(VolumeKeyProtectorID));
			System.Management.ManagementBaseObject outParams = volume.InvokeMethod("GetKeyProtectorNumericalPassword", inParams, null);
			NumericalPassword = outParams.Properties["NumericalPassword"].Value as string;
			return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
		}

		private static uint GetUInt32Property(ManagementObject volume, string methodName, string propertyName)
		{
			try
			{
				ManagementBaseObject result = volume.InvokeMethod(methodName, null, null);
				if (result != null && result[propertyName] != null)
				{
					return Convert.ToUInt32(result[propertyName]);
				}
			}
			catch { }
			return 0;
		}
	}
}