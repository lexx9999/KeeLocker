using System;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace KeeLockerAgent
{
	internal class OneShotElevatorCallee
	{
		private readonly string channelId;
		private readonly string callerSidString;
		private readonly string baseName;

		public OneShotElevatorCallee(string channelId, string callerSidString, string baseName)
		{
			this.channelId = channelId;
			this.callerSidString = callerSidString;
			this.baseName = baseName;
		}
		public int Run(Func<byte[], byte[]> handler)
		{
			SecurityIdentifier callerSid = new SecurityIdentifier(callerSidString);

			string pipeName = BuildPipeName(channelId);
			string eventName = BuildEventName(channelId);

			PipeSecurity pipeSecurity = BuildPipeSecurity(callerSid);

			NamedPipeServerStream server = null;
			EventWaitHandle readyEvent = null;

			try
			{
				server = new NamedPipeServerStream(
					pipeName,
					PipeDirection.InOut,
					1,
					PipeTransmissionMode.Byte,
					PipeOptions.None,
					4096,
					4096,
					pipeSecurity);

				// Signal readiness
				readyEvent = EventWaitHandle.OpenExisting(eventName);
				readyEvent.Set();

				// Wait for caller
				server.WaitForConnection();

				// 1) Read request FIRST
				byte[] request = ReadMessage(server);

				// 2) NOW impersonation is allowed
				if (!ValidateClientIdentity(server, callerSid))
					return 2;

				// 3) Process request
				byte[] response = handler(request);

				// 4) Send response
				WriteMessage(server, response);

				return 0;
			}
			finally
			{
				if (readyEvent != null)
					readyEvent.Dispose();
				if (server != null)
					server.Dispose();
			}
		}

		private  string BuildPipeName(string channelId)
		{
			return baseName+ ".Pipe." + channelId;
		}

		private  string BuildEventName(string channelId)
		{
			return baseName + ".Ready." + channelId;
		}

		private static PipeSecurity BuildPipeSecurity(SecurityIdentifier callerSid)
		{
			PipeSecurity ps = new PipeSecurity();

			ps.AddAccessRule(new PipeAccessRule(
				callerSid,
				PipeAccessRights.ReadWrite,
				AccessControlType.Allow));

			SecurityIdentifier system = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
			ps.AddAccessRule(new PipeAccessRule(
				system,
				PipeAccessRights.FullControl,
				AccessControlType.Allow));

			SecurityIdentifier admins = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
			ps.AddAccessRule(new PipeAccessRule(
				admins,
				PipeAccessRights.FullControl,
				AccessControlType.Allow));

			return ps;
		}

		private static bool ValidateClientIdentity(NamedPipeServerStream server, SecurityIdentifier expectedSid)
		{
			WindowsIdentity id = null;
			try
			{
				server.RunAsClient(delegate
				{
					id = WindowsIdentity.GetCurrent();
				});

				return id != null && id.User != null && id.User.Equals(expectedSid);
			}
			finally
			{
				if (id != null)
					id.Dispose();
			}
		}

		private static byte[] ReadMessage(Stream stream)
		{
			byte[] lengthBytes = ReadExact(stream, 4);
			int length = BitConverter.ToInt32(lengthBytes, 0);
			return ReadExact(stream, length);
		}

		private static void WriteMessage(Stream stream, byte[] data)
		{
			byte[] lengthBytes = BitConverter.GetBytes(data.Length);
			stream.Write(lengthBytes, 0, lengthBytes.Length);
			stream.Write(data, 0, data.Length);
			stream.Flush();
		}

		private static byte[] ReadExact(Stream stream, int count)
		{
			byte[] buffer = new byte[count];
			int offset = 0;

			while (offset < count)
			{
				int read = stream.Read(buffer, offset, count - offset);
				if (read <= 0)
					throw new EndOfStreamException();

				offset += read;
			}

			return buffer;
		}
	}
}
