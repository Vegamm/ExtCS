﻿using System;
using System.Text;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace DotNetDbg
{
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("88f7dfab-3ea7-4c3a-aefb-c4e8106173aa")]
	public unsafe interface IDebugDataSpaces
	{
		/* IDebugDataSpaces */
		[PreserveSig] int ReadVirtual(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] int WriteVirtual(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] int SearchVirtual(
		    [In] UInt64 Offset,
		    [In] UInt64 Length,
		    [In] IntPtr Pattern,
		    [In] UInt32 PatternSize,
		    [In] UInt32 PatternGranularity,
		    [Out] out UInt64 MatchOffset);
		[PreserveSig] int ReadVirtualUncached(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] int WriteVirtualUncached(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] int ReadPointersVirtual(
		    [In] UInt32 Count,
		    [In] UInt64 Offset,
		    [Out, MarshalAs(UnmanagedType.LPArray)] UInt64[] Ptrs);
		[PreserveSig] int WritePointersVirtual(
		    [In] UInt32 Count,
		    [In] UInt64 Offset,
		    [In, MarshalAs(UnmanagedType.LPArray)] UInt64[] Ptrs);
		[PreserveSig] int ReadPhysical(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] int WritePhysical(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] int ReadControl(
		    [In] UInt32 Processor,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] Int32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] int WriteControl(
		    [In] UInt32 Processor,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] Int32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] int ReadIo(
		    [In] INTERFACE_TYPE InterfaceType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 AddressSpace,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] int WriteIo(
		    [In] INTERFACE_TYPE InterfaceType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 AddressSpace,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] int ReadMsr(
		    [In] UInt32 Msr,
		    [Out] out UInt64 MsrValue);
		[PreserveSig] int WriteMsr(
		    [In] UInt32 Msr,
		    [In] UInt64 MsrValue);
		[PreserveSig] int ReadBusData(
		    [In] BUS_DATA_TYPE BusDataType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 SlotNumber,
		    [In] UInt32 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] int WriteBusData(
		    [In] BUS_DATA_TYPE BusDataType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 SlotNumber,
		    [In] UInt32 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] int CheckLowMemory();
		[PreserveSig] int ReadDebuggerData(
            [In] RDD_DEBUG_DATA Index,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* DataSize);
		[PreserveSig] int ReadProcessorSystemData(
		    [In] UInt32 Processor,
		    [In] DEBUG_DATA Index,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* DataSize);
	}
}
