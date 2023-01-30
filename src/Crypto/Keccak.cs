// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using Org.BouncyCastle.Crypto.Digests;

namespace HyperNebula.Crypto;

public class Keccak
{
    public static byte[] Hash256(byte[] data)
    {
        return Hash(data, 256);
    }
	
    public static byte[] Hash384(byte[] data)
    {
        return Hash(data, 384);
    }
	
    public static byte[] Hash512(byte[] data)
    {
        return Hash(data, 512);
    }

    public static byte[] Hash(byte[] data, uint bits)
    {
        byte[] result = new byte[bits/8];

        KeccakDigest hashAlgorithm = new((int)bits);
        hashAlgorithm.BlockUpdate(data, 0, data.Length);
        hashAlgorithm.DoFinal(result, 0);

        return result;
    }
}