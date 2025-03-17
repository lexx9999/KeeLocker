using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;


namespace KeeLocker
{
	public class FveApi
	{

		[StructLayout(LayoutKind.Explicit, Pack = 1, Size = 584)]
		internal struct FVE_AUTH_ELEMENT
		{
			[FieldOffset(0)]
			public Int32 MagicValue;

			[FieldOffset(4)]
			public Int32 MustBeOne;

			[FieldOffset(8)]
			public byte Data_Start;
		}

		[StructLayout(LayoutKind.Explicit, Pack = 1)]
		internal struct FVE_UNLOCK_SETTINGS
		{
			[FieldOffset(0x00)]
			public Int32 rsp_30;

			[FieldOffset(0x04)]
			public Int32 rsp_34;

			[FieldOffset(0x08)]
			public Int32 rsp_38;

			[FieldOffset(0x0C)]
			public Int32 rsp_3C;

			[FieldOffset(0x10)]
			public IntPtr rsp_40; // FVE_AUTH_ELEMENT**

			[FieldOffset(0x18)]
			public Int64 rsp_48;
		};

		internal enum HRESULT : int
		{
			S_OK = unchecked((int)0x00000000),
			S_FALSE = unchecked((int)0x00000001),
			FVE_E_FAILED_AUTHENTICATION = unchecked((int)0x80310027),
			FVE_E_NOT_ACTIVATED = unchecked((int)0x80310008),
		}

		internal static bool Succeeded(HRESULT hr)
		{
			return ((int)hr >= 0);
		}

		internal enum FVE_SECRET_TYPE
		{
			PassPhrase = unchecked((int)0x800000),
			RecoveryPassword = unchecked((int)0x80000),
		}

		[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "GetVolumeNameForVolumeMountPointW")]
		internal static extern bool GetVolumeNameForVolumeMountPoint(string lpszVolumeMountPoint, [Out] StringBuilder lpszVolumeName, uint cchBufferLength);

		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveAuthElementFromPassPhrase")]
		internal static extern HRESULT FveAuthElementFromPassPhrase(string PassPhrase, ref FVE_AUTH_ELEMENT AuthElement);
		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveAuthElementFromRecoveryPassword")]
		internal static extern HRESULT FveAuthElementFromRecoveryPassword(string PassPhrase, ref FVE_AUTH_ELEMENT AuthElement);
		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveOpenVolume")]
		internal static extern HRESULT FveOpenVolume(string VolumeId, Int32 FlagsMaybe, ref IntPtr HVolume);
		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveUnlockVolumeWithAccessMode")]
		internal static extern HRESULT FveUnlockVolumeWithAccessMode(IntPtr HVolume, ref FVE_UNLOCK_SETTINGS UnlockSettings, Int32 FlagsMaybe);
		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveCloseVolume")]
		internal static extern HRESULT FveCloseVolume(IntPtr HVolume, ref FVE_UNLOCK_SETTINGS UnlockSettings, Int32 FlagsMaybe, Int32 PassPhrase);

		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveCloseVolume")]
		internal static extern HRESULT FveCloseVolume(IntPtr HVolume, IntPtr ZeroUnlockSettings, Int32 FlagsMaybe, Int32 PassPhrase);

		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveGetAuthMethodGuids")]
		internal static extern HRESULT FveGetAuthMethodGuids(IntPtr HVolume, byte[] guids, uint maxGuids, ref uint GuidCount);
		[DllImport("FVEAPI.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "FveGetAuthMethodGuids")]
		internal static extern HRESULT FveGetAuthMethodGuidsQuery(IntPtr HVolume, IntPtr zeroPtr, uint maxGuids, ref uint GuidCount);

		[DllImport("kernel32.dll", SetLastError = true)]
		internal static extern uint QueryDosDevice(string lpDeviceName, [Out] StringBuilder lpTargetPath, uint ucchMax);

		[DllImport("kernel32.dll", EntryPoint = "FindFirstVolume", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern IntPtr FindFirstVolume([Out] StringBuilder lpszVolumeName, uint cchBufferLength);
		[DllImport("kernel32.dll", EntryPoint = "FindNextVolume", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern bool FindNextVolume(IntPtr hFindVolume, [Out] StringBuilder lpszVolumeName, uint cchBufferLength);
		[DllImport("kernel32.dll", EntryPoint = "FindVolumeClose", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern bool FindVolumeClose(IntPtr hFindVolume);

		[DllImport("kernel32.dll", EntryPoint = "FindFirstVolumeMountPoint", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern IntPtr FindFirstVolumeMountPoint(string lpszVolumeName, [Out] StringBuilder lpszVolumeMountPoint, uint cchBufferLength);
		[DllImport("kernel32.dll", EntryPoint = "FindNextVolumeMountPoint", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern bool FindNextVolumeMountPoint(IntPtr hFindVolumeMountPoint, [Out] StringBuilder lpszVolumeMountPoint, uint cchBufferLength);
		[DllImport("kernel32.dll", EntryPoint = "FindVolumeMountPointClose", SetLastError = true, CallingConvention = CallingConvention.StdCall)]
		internal static extern bool FindVolumeMountPointClose(IntPtr hFindVolumeMountPoint);

		[DllImport("kernel32.dll", EntryPoint = "GetVolumePathNamesForVolumeName", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetVolumePathNamesForVolumeName(string lpszVolumeName, [Out] char[] lpszVolumePathNames, uint cchBufferLength, ref UInt32 lpcchReturnLength);



		internal static bool GetVolumePathNamesForVolumeName(string lpszVolumeName, out List<string> volumePaths)
		{
			char[] buf = new char[20];
			//StringBuilder buf = new StringBuilder(10);
			UInt32 N = 0;
			if (!GetVolumePathNamesForVolumeName(lpszVolumeName, buf, (uint)buf.Length, ref N))
			{

				int ec = Marshal.GetLastWin32Error();
				if (ec == 0xea && N > 0)
				{
					//
					buf = new char[N];
					//buf = new StringBuilder((int)N);

					N = 0;
					if (!GetVolumePathNamesForVolumeName(lpszVolumeName, buf, (uint)buf.Length, ref N))
					{
						volumePaths = null;
						return false;
					}
				}
			}

			volumePaths = new List<string>();
			int z = 0;
			while (z < N && buf[z] != 0)
			{
				int q = Array.IndexOf(buf, '\0', z, (int)N - z);
				if (q == -1) break;
				volumePaths.Add(new string(buf, z, q));
				z += q + 1;
			}


			return true;


		}

		[Flags]
		public enum FileSystemFeature : uint
		{
			CasePreservedNames = 2,
			CaseSensitiveSearch = 1,
			DaxVolume = 0x20000000,
			FileCompression = 0x10,
			NamedStreams = 0x40000,
			PersistentACLS = 8,
			ReadOnlyVolume = 0x80000,
			SequentialWriteOnce = 0x100000,
			SupportsEncryption = 0x20000,
			SupportsExtendedAttributes = 0x00800000,
			SupportsHardLinks = 0x00400000,
			SupportsObjectIDs = 0x10000,
			SupportsOpenByFileId = 0x01000000,
			SupportsReparsePoints = 0x80,
			SupportsSparseFiles = 0x40,
			SupportsTransactions = 0x200000,
			SupportsUsnJournal = 0x02000000,
			UnicodeOnDisk = 4,
			VolumeIsCompressed = 0x8000,
			VolumeQuotas = 0x20
		}

		[DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public extern static bool GetVolumeInformation(string rootPathName, [Out] StringBuilder volumeNameBuffer, int volumeNameSize, out uint volumeSerialNumber,
			out uint maximumComponentLength, out FileSystemFeature fileSystemFlags, [Out] StringBuilder fileSystemNameBuffer, int nFileSystemNameSize);

		public enum Result
		{
			Ok,
			Unexpected,
			DriveNotFound,
			WrongPassPhrase,
			NoBitLockerVolume
		}

		private static IntPtr StructToPointer(object Struct)
		{
			IntPtr Pointer = Marshal.AllocHGlobal(Marshal.SizeOf(Struct));
			Marshal.StructureToPtr(Struct, Pointer, false);
			return Pointer;
		}

		public static bool GetDriveGUID(string DriveMountPoint, out string DriveGUID)
		{
			const int MaxVolumeNameLength = 50;
			StringBuilder DriveGUIDWriter = new StringBuilder(MaxVolumeNameLength);

			bool Ok = GetVolumeNameForVolumeMountPoint(DriveMountPoint, DriveGUIDWriter, (uint)DriveGUIDWriter.Capacity);
			if (!Ok)
			{
				DriveGUID = "";
				return false;
			}
			DriveGUID = DriveGUIDWriter.ToString();
			return true;
		}


		public static Result UnlockVolume(string DriveMountPoint, string DriveGUID, string PassPhrase, bool IsRecoveryKey)
		{
			Result R = Result.Ok;

			IntPtr pAuthElement = IntPtr.Zero;
			IntPtr ppAuthElement = IntPtr.Zero;
			IntPtr pUnlockSettings = IntPtr.Zero;
			do
			{
				if (DriveGUID.Length == 0)
				{
					bool Ok = GetDriveGUID(DriveMountPoint, out DriveGUID);
					if (!Ok)
					{
						R = Result.DriveNotFound;
						break;
					}
				}

				HRESULT HResult;

				FVE_AUTH_ELEMENT AuthElement = new FVE_AUTH_ELEMENT();
				Int32 SecretType;
				if (IsRecoveryKey)
				{
					SecretType = (Int32)FVE_SECRET_TYPE.RecoveryPassword;

					AuthElement.MagicValue = 32;
					AuthElement.MustBeOne = 1;
					HResult = FveAuthElementFromRecoveryPassword(PassPhrase, ref AuthElement);
					if (HResult != 0)
					{
						R = Result.WrongPassPhrase;
						break;
					}

				}
				else
				{
					SecretType = (Int32)FVE_SECRET_TYPE.PassPhrase;
					AuthElement.MagicValue = 578;
					AuthElement.MustBeOne = 1;
					HResult = FveAuthElementFromPassPhrase(PassPhrase, ref AuthElement);
					if (HResult != 0)
					{
						R = Result.WrongPassPhrase;
						break;
					}
				}

				IntPtr HVolume = IntPtr.Zero;
				HResult = FveOpenVolume(DriveGUID, 0, ref HVolume);
				if (HResult != 0)
				{
					R = Result.Unexpected;
					break;
				}


				pAuthElement = StructToPointer(AuthElement);
				ppAuthElement = StructToPointer(pAuthElement);

				FVE_UNLOCK_SETTINGS UnlockSettings = new FVE_UNLOCK_SETTINGS();
				UnlockSettings.rsp_30 = 56;
				UnlockSettings.rsp_34 = 1;
				UnlockSettings.rsp_38 = SecretType;
				UnlockSettings.rsp_3C = 1;
				UnlockSettings.rsp_40 = ppAuthElement;
				UnlockSettings.rsp_48 = 0;

				Int32 FlagsMaybe = 0;
				HResult = FveUnlockVolumeWithAccessMode(HVolume, ref UnlockSettings, FlagsMaybe);
				if (HResult == HRESULT.FVE_E_FAILED_AUTHENTICATION)
				{
					R = Result.WrongPassPhrase;
					break;
				}
				if (HResult == HRESULT.FVE_E_NOT_ACTIVATED)
				{
					R = Result.NoBitLockerVolume;
					break;
				}

				if (HResult != 0)
				{
					R = Result.Unexpected;
					break;
				}

				HResult = FveCloseVolume(HVolume, ref UnlockSettings, FlagsMaybe, SecretType);
				if (HResult != 0)
				{
					R = Result.Unexpected;
					break;
				}

				// Free the unmanaged memory.

			} while (false);

			if (ppAuthElement != (IntPtr)0)
				Marshal.FreeHGlobal(ppAuthElement);

			if (pAuthElement != (IntPtr)0)
				Marshal.FreeHGlobal(pAuthElement);

			return R;

		}

		public static bool TryQueryAuthGuids(string DriveMountPoint, out Guid[] guids)
		{
			uint N = 0;
			HRESULT HResult = 0;

			IntPtr HVolume = IntPtr.Zero;
			HResult = FveOpenVolume(DriveMountPoint, 0, ref HVolume);
			if (HResult != 0)
			{
				guids = null;
				return false;
			}

			List<Guid> result = new List<Guid>();

			HResult = FveGetAuthMethodGuids(HVolume, null, 0, ref N);
			if (!Succeeded(HResult) || N <= 0)
			{
				guids = null;
				return false;
			}

			var tmp = new byte[N * 16];
			HResult = FveGetAuthMethodGuids(HVolume, tmp, N, ref N);
			for (uint i = 0; i < N; i++)
			{
				byte[] g = new byte[16];
				Array.Copy(tmp, i * 16, g, 0, 16);
				result.Add(new Guid(g));
			}

			HResult = FveCloseVolume(HVolume, IntPtr.Zero, 0, 0);
			guids = result.ToArray();
			return true;
		}

		internal enum NTSTATUS
		{
			S_OK = unchecked((int)0x00000000),
		}

		internal const ulong WNF_FVE_STATE_CHANGE = 0x4183182BA3BC3875UL;


		[DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RtlSubscribeWnfStateChangeNotification")]
		internal static extern NTSTATUS RtlSubscribeWnfStateChangeNotification(out IntPtr Subscription, ulong StateName, int ChangeStamp, IntPtr Callback, IntPtr CallbackContext, IntPtr TypeId, int SerializationGroup, int Unknown);
		[DllImport("ntdll.dll", SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "RtlUnsubscribeWnfStateChangeNotification")]
		internal static extern NTSTATUS RtlUnsubscribeWnfStateChangeNotification(IntPtr Subscription);

		public delegate void TOnStateChangeDelegate();

		public delegate int TCallbackDelegate(
					ulong StateName,
					int ChangeStamp,
					IntPtr TypeId,
					IntPtr CallbackContext,
					IntPtr Buffer,
					int BufferSize);

		internal struct SCallbackContext
		{
			public TOnStateChangeDelegate Callback;
		};

		public struct SSubscription
		{
			public TOnStateChangeDelegate OnStateChange; // Have to keep that to prevent GC
			public IntPtr InternalPointer;
			public TCallbackDelegate Callback;
			public IntPtr CallbackContextPtr;

			public void Clear()
			{
				InternalPointer = IntPtr.Zero;
			}
			public bool IsValid()
			{
				return InternalPointer != IntPtr.Zero;
			}
		};



		static private int OnWnfStateChange(
			ulong stateName,
			int nChangeStamp,
			IntPtr pTypeId,
			IntPtr pCallbackContext,
			IntPtr pBuffer,
			int nBufferSize)
		{
			if (stateName != WNF_FVE_STATE_CHANGE)
				return 0;

			if (pCallbackContext == IntPtr.Zero) return 1;

			SCallbackContext CallbackContext = Marshal.PtrToStructure<SCallbackContext>(pCallbackContext);
			CallbackContext.Callback();
			return 0;
		}

		public static SSubscription StateChangeNotification_Subscribe(TOnStateChangeDelegate _OnStateChange)
		{
			TCallbackDelegate Callback = new TCallbackDelegate(OnWnfStateChange);

			SCallbackContext CallbackContext = new SCallbackContext();
			CallbackContext.Callback = _OnStateChange;

			IntPtr CallbackContextPtr = Marshal.AllocHGlobal(Marshal.SizeOf(CallbackContext));
			Marshal.StructureToPtr(CallbackContext, CallbackContextPtr, false);

			IntPtr pSubscription = IntPtr.Zero;
			NTSTATUS ntstatus = RtlSubscribeWnfStateChangeNotification(
				out pSubscription,
				WNF_FVE_STATE_CHANGE,
				0,
				Marshal.GetFunctionPointerForDelegate(Callback),
				CallbackContextPtr,
				IntPtr.Zero,
				0,
				0);

			SSubscription R;
			R.OnStateChange = _OnStateChange;
			R.InternalPointer = pSubscription;
			R.Callback = Callback;
			R.CallbackContextPtr = CallbackContextPtr;
			return R;
		}

		public static void StateChangeNotification_Unsubscribe(SSubscription Subscription)
		{
			if (Subscription.InternalPointer != IntPtr.Zero)
			{
				RtlUnsubscribeWnfStateChangeNotification(Subscription.InternalPointer);
				Marshal.FreeHGlobal(Subscription.CallbackContextPtr);
			}
		}
	}
}