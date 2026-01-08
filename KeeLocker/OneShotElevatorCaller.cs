using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;

namespace KeeLocker
{
	internal sealed class OneShotElevatorCaller
	{
		public byte[] Execute(byte[] request)
		{
			string channelId = Guid.NewGuid().ToString("N");
			string pipeName = BuildPipeName(channelId);
			string eventName = BuildEventName(channelId);
			string userSid = GetCurrentUserSid();

			EventWaitHandle readyEvent = null;
			bool created;

			readyEvent = new EventWaitHandle(
				false,
				EventResetMode.ManualReset,
				eventName,
				out created);

			StartElevatedAgent(channelId, userSid);

			// Wait for elevated agent to signal readiness
			bool signaled = readyEvent.WaitOne(60000);
			if (!signaled)
				throw new TimeoutException("Elevated agent did not signal readiness.");

			return CallAgent(pipeName, request);
		}

		private static string BuildPipeName(string channelId)
		{
			return Globals.APP_NAME + ".Pipe." + channelId;
		}

		private static string BuildEventName(string channelId)
		{
			return Globals.APP_NAME + ".Ready." + channelId;
		}

		private static string GetCurrentUserSid()
		{
			WindowsIdentity id = null;
			try
			{
				id = WindowsIdentity.GetCurrent();
				return id.User.Value;
			}
			finally
			{
				if (id != null)
					id.Dispose();
			}
		}

		private static void StartElevatedAgent(string channelId, string userSid)
		{
			string dllPath = typeof(OneShotElevatorCaller).Assembly.Location;
			string agentPath = Path.Combine(
				Path.GetDirectoryName(dllPath),
				Path.GetFileNameWithoutExtension(dllPath) + "Agent.exe");

			Process p = new Process();
			p.StartInfo = new ProcessStartInfo();
			p.StartInfo.UseShellExecute = true;
			p.StartInfo.Verb = "runas";
			p.StartInfo.FileName = agentPath;
			p.StartInfo.Arguments = channelId + " " + userSid;

			p.Start();
		}

		private static byte[] CallAgent(string pipeName, byte[] request)
		{
			NamedPipeClientStream client = null;
			try
			{
				client = new NamedPipeClientStream(
					".",
					pipeName,
					PipeDirection.InOut);

				client.Connect(5000);

				WriteMessage(client, request);
				// Thread.Sleep(5000);
				return ReadMessage(client);
			}
			finally
			{
				if (client != null)
					client.Dispose();
			}
		}

		private static void WriteMessage(Stream stream, byte[] data)
		{
			byte[] lengthBytes = BitConverter.GetBytes(data.Length);
			stream.Write(lengthBytes, 0, lengthBytes.Length);
			stream.Write(data, 0, data.Length);
			stream.Flush();
		}

		private static byte[] ReadMessage(Stream stream)
		{
			byte[] lengthBytes = ReadExact(stream, 4);
			int length = BitConverter.ToInt32(lengthBytes, 0);

			return ReadExact(stream, length);
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
