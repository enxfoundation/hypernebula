// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Globalization;

namespace HyperNebula.Common;

public static class EncodeUtil
{

	public static string ByteArrayToHexString(byte[] bytearray)
	{
		return "0x" + BitConverter.ToString(bytearray).Replace("-", string.Empty).ToLower();
	}

	public static byte[] Inverse(byte[] bytearray)
	{
		byte[] inverse = new byte[bytearray.Length];
		for (int i = 0; i < bytearray.Length; i++)
		{
			inverse[i] = (byte)(255-((UInt16)bytearray[i]));
		}

		return inverse;
	}
	
	public static string ByteArrayToHexString(byte[] bytearray, bool reverse)
	{
		if (reverse)
		{
			bytearray = bytearray.Reverse().ToArray();
		}
		IEnumerable<char> chararr = BitConverter.ToString(bytearray).Replace("-", string.Empty).ToLower();
		return "0x" + (string.Join("", chararr));
	}
	
	public static string ByteArrayToHexString(byte[] bytearray, bool reverse, uint fixed_length)
	{
		byte[] new_bytearray = new byte[fixed_length];
		for (uint i = 0; i < fixed_length; i++)
		{
			if (i < bytearray.Length)
			{
				new_bytearray[i] = bytearray[i];
			}
			else
			{
				new_bytearray[i] = 0x00;
			}
		}

		return ByteArrayToHexString(new_bytearray, reverse);
	}

	public static byte[] HexStringToByteArray(string hex_string)
	{
		if (hex_string.Substring(0, 2) == "0x")
		{
			hex_string = hex_string.Substring(2, hex_string.Length - 2);
		}

		// todo: verify hex_string % 2

		byte[] bytearray = new byte[hex_string.Length / 2];
		int offset = 0;
		for (int i = 0; i < hex_string.Length; i += 2)
		{
			ushort uint16 = ushort.Parse(hex_string.Substring(i, 2), NumberStyles.HexNumber);
			byte[] bytes = BitConverter.GetBytes(uint16);

			// todo: handle endianness. if system is big endian use bytes[1]. we want the least significant byte. 
			// most significant byte will always be 0x00
			byte hex_byte = bytes[0];

			bytearray[offset] = hex_byte;
			offset++;
		}

		return bytearray;
	}
}