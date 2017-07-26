using System.Runtime.InteropServices;
using System;

namespace UpgradeSolution1Support.PInvoke.SafeNative
{
	public static class kernel32
	{

		public static void CopyMemory(ref int pDest, int pSrc, int ByteLen)
		{
			GCHandle handle = GCHandle.Alloc(pDest, GCHandleType.Pinned);
			GCHandle handle2 = GCHandle.Alloc(pSrc, GCHandleType.Pinned);
			try
			{
				IntPtr tmpPtr2 = handle2.AddrOfPinnedObject();
				IntPtr tmpPtr = handle.AddrOfPinnedObject();
				UpgradeSolution1Support.PInvoke.UnsafeNative.kernel32.CopyMemory(tmpPtr, tmpPtr2, ByteLen);
				pSrc = Marshal.ReadInt32(tmpPtr2);
				pDest = Marshal.ReadInt32(tmpPtr);
			}
			finally
			{
				handle.Free();
				handle2.Free();
			}
		}
		public static void CopyMemory(ref byte pDest, ref float pSrc, int ByteLen)
		{
			GCHandle handle = GCHandle.Alloc(pDest, GCHandleType.Pinned);
			GCHandle handle2 = GCHandle.Alloc(pSrc, GCHandleType.Pinned);
			try
			{
				IntPtr tmpPtr2 = handle2.AddrOfPinnedObject();
				//UPGRADE_WARNING: (8007) Trying to marshal a non Bittable Type (float). A special conversion might be required at this point. Moreover use 'External Marshalling attributes for Structs' feature enabled if required More Information: http://www.vbtonet.com/ewis/ewi8007.aspx
				IntPtr tmpPtr = handle.AddrOfPinnedObject();
				UpgradeSolution1Support.PInvoke.UnsafeNative.kernel32.CopyMemory(tmpPtr, tmpPtr2, ByteLen);
				pDest = Marshal.ReadByte(tmpPtr);
			}
			finally
			{
				handle.Free();
				handle2.Free();
			}
		}
	}
}