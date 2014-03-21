﻿using System;
using System.Text;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace DotNetDbg
{
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("7a5e852f-96e9-468f-ac1b-0b3addc4a049")]
	public unsafe interface IDebugDataSpaces2 : IDebugDataSpaces
	{
		/* IDebugDataSpaces */
		[PreserveSig] new int ReadVirtual(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] new int WriteVirtual(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] new int SearchVirtual(
		    [In] UInt64 Offset,
		    [In] UInt64 Length,
		    [In] IntPtr Pattern,
		    [In] UInt32 PatternSize,
		    [In] UInt32 PatternGranularity,
		    [Out] out UInt64 MatchOffset);
		[PreserveSig] new int ReadVirtualUncached(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] new int WriteVirtualUncached(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] new int ReadPointersVirtual(
		    [In] UInt32 Count,
		    [In] UInt64 Offset,
		    [Out, MarshalAs(UnmanagedType.LPArray)] UInt64[] Ptrs);
		[PreserveSig] new int WritePointersVirtual(
		    [In] UInt32 Count,
		    [In] UInt64 Offset,
		    [In, MarshalAs(UnmanagedType.LPArray)] UInt64[] Ptrs);
		[PreserveSig] new int ReadPhysical(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] new int WritePhysical(
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] new int ReadControl(
		    [In] UInt32 Processor,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] Int32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] new int WriteControl(
		    [In] UInt32 Processor,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] Int32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] new int ReadIo(
		    [In] INTERFACE_TYPE InterfaceType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 AddressSpace,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] new int WriteIo(
		    [In] INTERFACE_TYPE InterfaceType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 AddressSpace,
		    [In] UInt64 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] new int ReadMsr(
		    [In] UInt32 Msr,
		    [Out] out UInt64 MsrValue);
		[PreserveSig] new int WriteMsr(
		    [In] UInt32 Msr,
		    [In] UInt64 MsrValue);
		[PreserveSig] new int ReadBusData(
		    [In] BUS_DATA_TYPE BusDataType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 SlotNumber,
		    [In] UInt32 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesRead);
		[PreserveSig] new int WriteBusData(
		    [In] BUS_DATA_TYPE BusDataType,
		    [In] UInt32 BusNumber,
		    [In] UInt32 SlotNumber,
		    [In] UInt32 Offset,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* BytesWritten);
		[PreserveSig] new int CheckLowMemory();
		[PreserveSig] new int ReadDebuggerData(
            [In] RDD_DEBUG_DATA Index,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* DataSize);
		[PreserveSig] new int ReadProcessorSystemData(
		    [In] UInt32 Processor,
		    [In] DEBUG_DATA Index,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* DataSize);

		/* IDebugDataSpaces2 */
		[PreserveSig] int VirtualToPhysical(
		    [In] UInt64 Virtual,
		    [Out] out UInt64 Physical);
		[PreserveSig] int GetVirtualTranslationPhysicalOffsets(
		    [In] UInt64 Virtual,
		    [Out, MarshalAs(UnmanagedType.LPArray)] UInt64[] Offsets,
		    [In] UInt32 OffsetsSize,
		    [In] UInt32* Levels);
		[PreserveSig] int ReadHandleData(
		    [In] UInt64 Handle,
		    [In] DEBUG_HANDLE_DATA_TYPE DataType,
		    [In] IntPtr Buffer,
		    [In] UInt32 BufferSize,
		    [In] UInt32* DataSize);
		[PreserveSig] int FillVirtual(
		    [In] UInt64 Start,
		    [In] UInt32 Size,
		    [In] IntPtr Pattern,
		    [In] UInt32 PatternSize,
		    [In] UInt32* Filled);
		[PreserveSig] int FillPhysical(
		    [In] UInt64 Start,
		    [In] UInt32 Size,
		    [In] IntPtr Pattern,
		    [In] UInt32 PatternSize,
		    [In] UInt32* Filled);
		[PreserveSig] int QueryVirtual(
		    [In] UInt64 Offset,
			[In] IntPtr Info_Aligned_MEMORY_BASIC_INFORMATION64);
		    //[Out] out MEMORY_BASIC_INFORMATION64 Info);
	}
}
