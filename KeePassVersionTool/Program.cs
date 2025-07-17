using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KeePassVersionTool
{
  internal class Program
  {
    static int Main(string[] args)
    {
      // see https://keepass.info/help/v2_dev/plg_index.html
      switch (args[0].ToLowerInvariant())
      {
        case "create":
          CreateRSAPair();
          return 0;
        case "validate":
          return SignOrValidateVersionFile(args, false);
        case "sign":
          return SignOrValidateVersionFile(args, true);
        default:
          Console.WriteLine("Unknown parameter {}", args[0]);
          return 1;
      }
    }


    private static int SignOrValidateVersionFile(string[] args, bool sign)
    {
      string KeyData = null;
      string VersionFile = null;

      for (int a = 0; a < args.Length; a++)
      {
        if (args[a].StartsWith("/") || args[a].StartsWith("-"))
        {
          switch (args[a].Substring(1).ToLowerInvariant())
          {
            case "file":
              if (VersionFile != null) throw new Exception("Duplicate version file specified");
              a++;
              if (args.Length <= a) throw new ArgumentException("Argument for key missing");

              VersionFile = args[a].Trim();
              break;
            case "key":
              if (KeyData != null) throw new Exception("Duplicate Key specified");
              a++;
              if (args.Length <= a) throw new ArgumentException("Argument for key missing");
              KeyData = args[a].Trim();
              break;

            case "keyfile":
              if (KeyData != null) throw new Exception("Duplicate Key specified");
              a++;
              if (args.Length <= a) throw new ArgumentException("Argument for key missing");
              KeyData = File.ReadAllText(args[a], Encoding.UTF8).Trim();
              break;
            case "keyenv":
              if (KeyData != null) throw new Exception("Duplicate Key specified");
              a++;
              if (args.Length <= a) throw new ArgumentException("Argument for key missing");
              KeyData = System.Environment.GetEnvironmentVariable(args[a]).Trim();
              break;
            case "randomkey":
              if (KeyData != null) throw new Exception("Duplicate Key specified");
              var kv = CreateRSAPair();
              KeyData = kv.Key;
              break;
          }
        }
      }

      if (KeyData == null || KeyData.Length == 0) throw new Exception("Missing private key parameter");
      if (VersionFile == null || VersionFile.Length == 0) throw new Exception("Missing version file parameter");

      int rc = SignOrValidateVersionFile(VersionFile, KeyData, sign);
      if (sign && rc == 0)
        rc = SignOrValidateVersionFile(VersionFile, KeyData, false);
      return rc;
    }

    private static int SignOrValidateVersionFile(string VersionFile,string KeyData, bool sign)
    {
      string[] text = File.ReadAllLines(VersionFile, Encoding.UTF8);

      string signature = null;
      StringBuilder sb = new StringBuilder();
      List<string> lines = new List<string>();

      foreach (string line in text)
      {
        string t = line.Trim();
        if (t.StartsWith(":"))
        {
          if (signature == null)
            signature = t.Substring(1).Trim();
          continue;
        }
        if (t.Length == 0)
          continue;

        lines.Add(t);
        sb.Append(t);
        sb.Append('\n');
      }
      string normalized = sb.ToString();

      if (sign)
      {
        using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
        {
          rsa.PersistKeyInCsp = false;
          rsa.FromXmlString(KeyData);
          if (sign && rsa.PublicOnly) throw new Exception("Signing requires the private key");

          var encUtf8 = new UTF8Encoding(false, false);

          try
          {
            byte[] pbMsg = encUtf8.GetBytes(normalized);
            // byte[] pbSig = Convert.FromBase64String(strSig);
            using (SHA512Managed sha = new SHA512Managed())
            {
              byte[] sigbytes = rsa.SignData(pbMsg, sha);
              signature = Convert.ToBase64String(sigbytes);
              lines.Insert(0, ":" + signature);
              lines.Add(":");

              WriteAllLinesUnix(VersionFile, lines.ToArray());
            }


          }
          catch
          {
            return 1;
          }
        }
        return 0;
      }

      bool ok = VerifySignature(normalized, signature, KeyData);
      Console.WriteLine(ok ? "Signature Valid" : "Invalid signature");
      return ok ? 0 : 2;


    }

    public static void WriteAllLinesUnix(string path, string[] contents)
    {
      if (path == null)
      {
        throw new ArgumentNullException("path");
      }

      if (contents == null)
      {
        throw new ArgumentNullException("contents");
      }

      if (path.Length == 0)
      {
        throw new ArgumentException("EmptyPath");
      }
      UTF8Encoding uTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);

      using (var writer=new StreamWriter(path, append: false, uTF8NoBOM))
      {
        writer.NewLine= "\n";
        foreach (string content in contents)
        {
          writer.WriteLine(content);
        }
      }
    }

    private static bool VerifySignature(string strContent, string strSig,
    string strKey)
    {
      if (string.IsNullOrEmpty(strSig)) { Debug.Assert(false); return false; }
      var encUtf8= new UTF8Encoding(false, false); 

      try
      {
        byte[] pbMsg = encUtf8.GetBytes(strContent);
        byte[] pbSig = Convert.FromBase64String(strSig);

        using (SHA512Managed sha = new SHA512Managed())
        {
          using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
          {
            // Watching this code in the debugger may result in a
            // CryptographicException when disposing the object
            rsa.PersistKeyInCsp = false; // Default key
            rsa.FromXmlString(strKey);
            rsa.PersistKeyInCsp = false; // Loaded key

            if (!rsa.VerifyData(pbMsg, sha, pbSig))
            {
              Debug.Assert(false);
              return false;
            }

            rsa.PersistKeyInCsp = false;
          }
        }
      }
      catch (Exception) { Debug.Assert(false); return false; }

      return true;
    }


    private static KeyValuePair<string,string> CreateRSAPair()
    {
      Console.WriteLine("Creating key...");

      using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
      {
        rsa.PersistKeyInCsp = false;
        string private_key = rsa.ToXmlString(true);
        string public_key = rsa.ToXmlString(false);

        Console.WriteLine("Private key:");
        Console.WriteLine(private_key);
        Console.WriteLine();

        Console.WriteLine("Public key:");
        Console.WriteLine(public_key);
        Console.WriteLine();
        Console.WriteLine("Done.");
        return new KeyValuePair<string, string>(private_key, public_key);
      }
    }

  }
}