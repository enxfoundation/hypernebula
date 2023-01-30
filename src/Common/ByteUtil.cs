// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

namespace HyperNebula.Common;

public static class ByteUtil
{
	public static byte[] ConcatListToByteArray(List<byte[]> bytes_list)
	{
		ByteArrayBuilder builder = new();
		foreach (byte[] block_hash in bytes_list)
		{
			builder.Append(block_hash);
		}

		return builder.Build();
	}

	public static List<byte[]> SplitToList(byte[] bytearray, uint len)
	{
		// todo: fix. this can cause poor performance
		return SplitToHashSet(bytearray, len).ToList();
	}

	/// <summary>
	/// Split a byte array into a list of byte arrays
	/// </summary>
	/// <param name="bytearray"></param>
	/// <param name="count"></param>
	/// <returns></returns>
	public static HashSet<byte[]> SplitToHashSet(byte[] bytearray, uint len)
	{
		HashSet<byte[]> hashset = new(new ByteEqualityComparer());
            
		if (bytearray.Length < len || len <= 0)
		{
			return new HashSet<byte[]>();
		}

		byte[] empty = new byte[len];
		
		byte[] block_hash;
		for (uint offset = 0; offset < bytearray.Length;)
		{
			if (offset + len > bytearray.Length)
			{
				return hashset;
			}

			block_hash = ByteUtil.Subset(bytearray, offset, len);

			if (ByteUtil.Equals(block_hash, empty))
			{
				return hashset;
			}

			hashset.Add(block_hash);
			offset += len;
		}

		return hashset;
	}

	public static bool Contains(HashSet<byte[]> hashset, byte[] bytes)
	{
		foreach (byte[] b in hashset)
		{
			if (Equals(b, bytes))
			{
				return true;
			}
		}

		return false;
	}

	public static bool Contains(List<byte[]> hashset, byte[] bytes)
	{
		foreach (byte[] b in hashset)
		{
			if (Equals(b, bytes))
			{
				return true;
			}
		}

		return false;
	}

	public static byte[] AppendBytes(byte[] bytearray, byte b)
	{
		byte[] append_bytearray = new byte[1] { b };
		return AppendBytes(bytearray, append_bytearray);
	}

	// bytes_to_append ensures exactly this many bytes are appended. if insufficient bytes in bytearray b,
	//   then will append 0x00 until length is reached.
	public static byte[] AppendBytes(byte[] bytearray, byte[] b, uint bytes_to_append)
	{
		uint delta = bytes_to_append - (uint)b.Length;

		byte[] append_bytes = new byte[delta];
		byte[] new_b = AppendBytes(b, append_bytes);
		return AppendBytes(bytearray, new_b);
	}

	// Initial byte array must always be empty
	public static byte[] AppendBytes(byte[] bytearray, byte[] b)
	{
		int new_bytearray_size = bytearray.Length + b.Length;
		byte[] prev_bytearray = bytearray;
		bytearray = new byte[new_bytearray_size];

		for (int i = 0; i < new_bytearray_size; i++)
		{
			if (i < prev_bytearray.Length)
			{
				bytearray[i] = prev_bytearray[i];
			}
			else
			{
				bytearray[i] = b[i - prev_bytearray.Length];
			}
		}

		return bytearray;
	}

	public static byte[] Concat(byte[] a, byte[] b)
	{
		byte[] x = new byte[a.Length + b.Length];
		Array.Copy(a, 0, x, 0, a.Length);
		Array.Copy(b, 0, x, a.Length, b.Length);
		return x;
	}

	public static byte[] Subset(byte[] bytearray, uint start, uint length)
	{
		byte[] subset = new byte[length];
		for (int i = 0; i < length; i++)
		{
			subset[i] = bytearray[start + i];
		}

		return subset;
	}

	public static bool Equals(byte[] a, byte[] b)
	{
		if (a.Length != b.Length)
		{
			return false;
		}

		for (int i = 0; i < a.Length; i++)
		{
			if (a[i] != b[i])
			{
				return false;
			}
		}

		return true;
	}

	public static byte[] Reverse(byte[] b)
	{
		byte[] reversed_bytearray = new byte[b.Length];
		for (int i = b.Length - 1; i >= 0; i--)
		{
			reversed_bytearray[i] = b[b.Length - i - 1];
		}

		return reversed_bytearray;
	}

	private static byte[] RemoveInsignificantBits(byte[] b)
	{
		int i;
		int nonzeros_found = 0;
		for (i = 0; i < b.Length && nonzeros_found < 1; i++)
		{
			if (b[i] != 0x00)
			{
				nonzeros_found++;
			}
		}

		for (; i < b.Length; i++)
		{
			b[i] = 0xff;
		}

		return b;
	}

	public static byte[] Inverse(byte[] b)
	{
		byte[] inverse_bytearray = new byte[b.Length];
		
		for (int i = 0; i < b.Length; i++)
		{
			inverse_bytearray[i] = (byte)(255 - b[i]);
		}

		return inverse_bytearray;
	}
}