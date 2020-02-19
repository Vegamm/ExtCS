﻿using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace DotNetDbg
{
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("9f50e42c-f136-499e-9a97-73036c94ed2d")]
	public interface IDebugInputCallbacks
	{
		[PreserveSig] int StartInput(
			[In] UInt32 BufferSize);
		[PreserveSig] int EndInput();
	}
}
