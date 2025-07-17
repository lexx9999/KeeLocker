using System.Xml.Serialization;
using System.IO;
using System;
using System.Collections.Generic;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace TestBitLockerWMI2
{
  [Serializable]
  public class KeyProtectorInfo
  {
    [XmlAttribute]
    public string ID { get; set; }
    [XmlAttribute]
    public uint Type { get; set; }
  }
  [Serializable]
  public class BitLockerVolumeInfo
  {
    [XmlAttribute]
    public string DriveLetter { get; set; }
    [XmlAttribute]
    public string VolumeID { get; set; }
    [XmlAttribute]
    public string PersistentVolumeID { get; set; }
    [XmlAttribute]
    public uint VolumeType { get; set; }
    [XmlAttribute]
    public uint ProtectionStatus { get; set; }
    [XmlAttribute]
    public uint EncryptionMethod { get; set; }
    [XmlAttribute]
    public uint ConversionStatus { get; set; }

    [XmlArray("KeyProtectors")]
    [XmlArrayItem("KeyProtector")]
    public List<KeyProtectorInfo> KeyProtectors { get; set; }
  }

  [Serializable]
  public class KeeLockerData
  {
    [XmlAttribute]
    public string Version { get; set; }
    [XmlAttribute]
    public string Time { get; set; }


    [XmlArray("BitLockerVolumeInfo")]
    [XmlArrayItem("Volume")]
    public List<BitLockerVolumeInfo> BitLockerVolumeInfos { get; set; }
  }

  class Program
  {
    [STAThread]
    static void Main(string[] args)
    {
      string base64Key = GetArgument(args, "/password");
      if (string.IsNullOrEmpty(base64Key))
      {
        Console.WriteLine("{\"error\":\"Missing /password parameter\"}");
        return;
      }



      byte[] key;
      try
      {
        key = Convert.FromBase64String(base64Key);
      }
      catch (Exception x)
      {
        Console.WriteLine("{\"error\":\"Invalid base64 password\"}");
        return;
      }

      if (key.Length != 16 && key.Length != 24 && key.Length != 32)
      {
        Console.WriteLine("{\"error\":\"Password must be 16, 24, or 32 bytes after base64 decoding\"}");
        return;
      }

      List<BitLockerVolumeInfo> volumes = GetBitLockerVolumes();
      KeeLockerData kld = new KeeLockerData();
      kld.BitLockerVolumeInfos = volumes;
      kld.Version = "KeeLocker-Data-V1";
      kld.Time = DateTime.UtcNow.ToString("o");



      XmlSerializer serializer = new XmlSerializer(typeof(KeeLockerData));
      StringWriter writer = new StringWriter();
      serializer.Serialize(writer, kld);
      string encrypted = EncryptString(writer.ToString(), key);
      Console.WriteLine(encrypted);
      Console.WriteLine(DecryptString(encrypted, key));
      System.Windows.Forms.Clipboard.SetText(encrypted);
      System.Windows.Forms.Clipboard.SetText(writer.ToString());
    }

    static string GetArgument(string[] args, string name)
    {
      for (int i = 0; i < args.Length - 1; i++)
      {
        if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase))
          return args[i + 1];
      }
      return null;
    }

    static string EncryptString(string plainText, byte[] key)
    {
      using (Aes aes = Aes.Create())
      {
        aes.Key = key;
        aes.GenerateIV(); // Random IV for each run

        using (var encryptor = aes.CreateEncryptor())
        {
          byte[] inputBytes = Encoding.UTF8.GetBytes(plainText);
          byte[] encryptedBytes = encryptor.TransformFinalBlock(inputBytes, 0, inputBytes.Length);

          // Combine IV + ciphertext
          byte[] result = new byte[aes.IV.Length + encryptedBytes.Length];
          Array.Copy(aes.IV, 0, result, 0, aes.IV.Length);
          Array.Copy(encryptedBytes, 0, result, aes.IV.Length, encryptedBytes.Length);

          return Convert.ToBase64String(result);
        }
      }
    }

    static string DecryptString(string encryptedBase64, byte[] key)
    {
      byte[] fullCipher = Convert.FromBase64String(encryptedBase64);

      using (Aes aes = Aes.Create())
      {
        aes.Key = key;

        // Extract IV (first 16 bytes)
        byte[] iv = new byte[16];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        aes.IV = iv;

        // Extract ciphertext
        int cipherLength = fullCipher.Length - iv.Length;
        byte[] cipherText = new byte[cipherLength];
        Array.Copy(fullCipher, iv.Length, cipherText, 0, cipherLength);

        using (ICryptoTransform decryptor = aes.CreateDecryptor())
        {
          byte[] decryptedBytes = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);
          return Encoding.UTF8.GetString(decryptedBytes);
        }
      }
    }

    static List<BitLockerVolumeInfo> GetBitLockerVolumes()
    {
      List<BitLockerVolumeInfo> volumes = new List<BitLockerVolumeInfo>();

      try
      {
        ManagementScope scope = new ManagementScope(@"\\.\root\CIMV2\Security\MicrosoftVolumeEncryption");
        scope.Connect();

        ObjectQuery query = new ObjectQuery("SELECT * FROM Win32_EncryptableVolume");
        ManagementObjectSearcher searcher = new ManagementObjectSearcher(scope, query);

        foreach (ManagementObject volume in searcher.Get())
        {
          BitLockerVolumeInfo info = new BitLockerVolumeInfo();

          info.DriveLetter = volume["DriveLetter"] != null ? volume["DriveLetter"].ToString() : "";
          info.VolumeID = volume["DeviceID"] != null ? volume["DeviceID"].ToString() : "";
          info.PersistentVolumeID = volume["PersistentVolumeID"] != null ? volume["PersistentVolumeID"].ToString() : "";
          info.VolumeType = volume["VolumeType"] != null ? Convert.ToUInt32(volume["VolumeType"]) : 0;

          info.ProtectionStatus = GetRawStatus(volume, "GetProtectionStatus", "ProtectionStatus");
          info.EncryptionMethod = GetRawStatus(volume, "GetEncryptionMethod", "EncryptionMethod");
          info.ConversionStatus = GetRawStatus(volume, "GetConversionStatus", "ConversionStatus");

          info.KeyProtectors = new List<KeyProtectorInfo>();

          try
          {
            string[] ids;

            uint kk = GetKeyProtectors(volume, 0, out ids);
            foreach (string protectorId in ids)
            {
              uint KeyProtectorType;
              uint kj = GetKeyProtectorType(volume, protectorId, out KeyProtectorType);

              info.KeyProtectors.Add(new KeyProtectorInfo
              {
                ID = protectorId,
                Type = KeyProtectorType
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

    static uint GetKeyProtectors(ManagementObject volume, uint KeyProtectorType, out string[] VolumeKeyProtectorID)
    {
      System.Management.ManagementBaseObject inParams = null;
      inParams = volume.GetMethodParameters("GetKeyProtectors");
      inParams["KeyProtectorType"] = ((uint)(KeyProtectorType));
      System.Management.ManagementBaseObject outParams = volume.InvokeMethod("GetKeyProtectors", inParams, null);
      VolumeKeyProtectorID = ((string[])(outParams.Properties["VolumeKeyProtectorID"].Value));
      return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
    }
    static uint GetKeyProtectorType(ManagementObject volume, string VolumeKeyProtectorID, out uint KeyProtectorType)
    {
      System.Management.ManagementBaseObject inParams = null;
      inParams = volume.GetMethodParameters("GetKeyProtectorType");
      inParams["VolumeKeyProtectorID"] = ((string)(VolumeKeyProtectorID));
      System.Management.ManagementBaseObject outParams = volume.InvokeMethod("GetKeyProtectorType", inParams, null);
      KeyProtectorType = System.Convert.ToUInt32(outParams.Properties["KeyProtectorType"].Value);
      return System.Convert.ToUInt32(outParams.Properties["ReturnValue"].Value);
    }

    static uint GetRawStatus(ManagementObject volume, string methodName, string propertyName)
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
