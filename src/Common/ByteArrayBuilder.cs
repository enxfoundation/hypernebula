// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

namespace HyperNebula.Common;

public class ByteArrayBuilder
{
    private byte[] bytearray = { };
    private readonly uint max_size;

    public ByteArrayBuilder(uint max_size)
    {
        this.max_size = max_size;
    }

    public ByteArrayBuilder() { }

    public byte[] Build()
    {
        return bytearray;
    }

    public byte[] GetBytes()
    {
        return bytearray;
    }

    public void Append(byte[] data, uint length)
    {
        if (data == null)
        {
            throw new Exception("ByteArrayBuilder: null data encountered");
        }
        if (data.Length != length)
        {
            throw new Exception("ByteArrayBuilder: data length does not match expected length. length=" + data.Length +
                                ", expected_length=" + length);
        }
        Append(data);
    }
	
    public void Append(byte[]? bytes)
    {
        if(bytes == null)
        {
            return;
        }
		
        if (max_size > 0 && bytearray.Length + bytes.Length > max_size)
        {
            throw new Exception("Unable to append - would exceed max_size");
        }

        bytearray = ByteUtil.AppendBytes(bytearray, bytes);
    }

    public void Append(byte b)
    {
        if (max_size > 0 && bytearray.Length + 1 > max_size)
        {
            throw new Exception("Unable to append - would exceed max_size");
        }
		
        bytearray = ByteUtil.AppendBytes(bytearray, b);
    }

    public void Append(ushort uint16)
    {
        Append(BitConverter.GetBytes(uint16));
    }

    public void Append(uint uint32)
    {
        Append(BitConverter.GetBytes(uint32));
    }

    public void Append(UInt64 uint64)
    {
        Append(BitConverter.GetBytes(uint64));
    }
}