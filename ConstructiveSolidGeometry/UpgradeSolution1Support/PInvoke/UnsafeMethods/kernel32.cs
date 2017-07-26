using System.Runtime.InteropServices;
using System;

namespace UpgradeSolution1Support.PInvoke.UnsafeNative
{
	[System.Security.SuppressUnmanagedCodeSecurity]
	public static class kernel32
	{

		[DllImport("kernel32.dll", EntryPoint = "RtlMoveMemory", CharSet = CharSet.Ansi, SetLastError = true, ExactSpelling = true)]
		extern public static void CopyMemory(System.IntPtr pDest, System.IntPtr pSrc, int ByteLen);
	}
}