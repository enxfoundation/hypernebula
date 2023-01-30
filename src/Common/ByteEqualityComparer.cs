// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using K4os.Hash.xxHash;

namespace HyperNebula.Common;

public class ByteEqualityComparer : IEqualityComparer<byte[]>
{
    public bool Equals(byte[]? a, byte[]? b)
    {
        if (a == null && b == null)
            return true;
        if (a == null || b == null)
            return false;
        if (a.Length != b.Length)
            return false;

        for (int i = 0; i < a.Length; i++)
        {
            if (a[i] != b[i])
            {
                return false;
            }
        }

        return true;
    }

    public int GetHashCode(byte[] x)
    {
        // false positives are possible, this is a bloom filter pre-check
        uint hash = XXH32.DigestOf(x);
        byte[] bytes = BitConverter.GetBytes(hash);
        return BitConverter.ToInt32(bytes, 0);
    }
}