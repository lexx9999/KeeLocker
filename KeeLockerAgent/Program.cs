using System;
using System.IO;

namespace KeeLockerAgent
{
	using System.Text;
	using System.Threading;
	using System.Xml.Serialization;
	using KeeLocker.BitLockerWMI;

	internal class Program
	{
		[STAThread]
		static int Main(string[] args)
		{
		//	Debugger.Launch();
			if (args.Length < 2)
				return 1;

			string channelId = args[0];
			string callerSid = args[1];
			var callee=new OneShotElevatorCallee(channelId, callerSid, "KeeLocker");

			int rc= callee.Run(request =>
			{
				// request → response
				return ProcessRequest(request);
			});
			Thread.Sleep(5000);
			return rc;
		}

		private static byte[] ProcessRequest(byte[] request)
		{
			string cmd = UTF8Encoding.UTF8.GetString(request, 0, request.Length);
			if (cmd != "ScanBitLockerDrives")
				return new byte[0];

			var scanInfo = new ScanInfo
			{
				Volumes = BitLocker.GetBitLockerVolumes(),
				MachineId = Util.GetMachineGuid(),
				Creator = "Agent"
			};

			XmlSerializer serializer = new XmlSerializer(scanInfo.GetType());
			using (MemoryStream writer = new MemoryStream())
			{
				serializer.Serialize(writer, scanInfo);
				var data= writer.ToArray();
				//System.Windows.Forms.Clipboard.SetText(UTF8Encoding.UTF8.GetString(data));
				return data;
			}
		}
	}
}
