using System;
using System.Collections.Generic;

namespace Lynk.Devices.Shared
{
	public interface II2cDevice
	{
		void Read(Span<byte> readBuffer);

		byte Read();
	

		/// <summary>
		/// Writes to the device then reads the data into
		/// </summary>
		/// <param name="writeBuffer">Writes multiple bytes to the device</param>
		/// <param name="readBuffer">The data read from the device</param>
		void WriteRead(ReadOnlySpan<byte> writeBuffer, Span<byte> readBuffer);
		
		/// <summary>
		/// Writes multiple bytes to the device
		/// </summary>
		/// <param name="writeBuffer">byte array to write</param>
		void Write(ReadOnlySpan<byte> writeBuffer);

		/// <summary>
		/// Writes a single byte to the device
		/// </summary>
		/// <param name="value">byte to write</param>
		void Write(byte value);
	}
}
