// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using System.Numerics;
using HyperNebula.Common;
using HyperNebula.Crypto;

namespace HyperNebula;

public static class HyperNebulaProtocol
{
    /// <summary>
    /// Protocol Version
    /// </summary>
    public const byte VERSION = 0x01;
    
    /// <summary>
    /// Node ID bit size
    /// </summary>
    public const int NODE_ID_BITS = 128;

    /// <summary>
    /// H is the maximum size of an H-Bucket, and should be chosen such
    /// that any given H nodes are unlikely to fail simultaneously.
    /// </summary>
    public const int H = 8; // 20

    /// <summary>
    /// Maximum bytes to read from remote node
    /// </summary>
    public const int MAX_READ_BYTES = 65535;
    
    /// <summary>
    /// Maximum bytes to read from remote node when we have not
    /// sent a request
    /// </summary>
    public const int MAX_REQUEST_READ_BYTES = 128;
    
    /// <summary>
    /// This signifies an IPV6 IP Address
    /// </summary>
    public const byte IPV6_FLAG = 0x01;
    
    /// <summary>
    /// This signifies an IPV4 IP Address
    /// </summary>
    public const byte IPV4_FLAG = 0xFF;
    
    /// <summary>
    /// Calculate the distance between nodes A and B
    /// </summary>
    /// <param name="a">ID of H-Node A</param>
    /// <param name="b">ID of H-Node B</param>
    /// <returns>Distance between the nodes as a UInt128</returns>
    public static UInt128 CalculateDistance(UInt128 a, UInt128 b)
    {
        if (a.CompareTo(b) > 0)
        {
            return a - b;
        }

        return b - a;
    }

    /// <summary>
    /// By evaluating whether distance fits in specified H-Bucket bounds, select an h-bucket
    /// for the specified node.
    /// </summary>
    /// <param name="distance"></param>
    /// <param name="min_range"></param>
    /// <param name="max_range"></param>
    /// <returns></returns>
    /// todo: maybe rename SortIntoHBucket()
    public static UInt32 SelectNodeHBucketId(UInt128 distance)
    {
        if (distance == 0)
        {
            return 0;
        }
        
        // NODE_ID_BITS h-buckets, one for each bit of node_id
        for (UInt32 hbucket_id = 0; hbucket_id < HyperNebulaProtocol.NODE_ID_BITS; hbucket_id++)
        {
            // Calculate lower bounds of the H-Bucket
            BigInteger pow = BigInteger.Pow(2, (int)hbucket_id);
            UInt128 min_range = UInt128.Parse(pow.ToString());

            // Calculate the upper bounds of the H-Bucket
            pow = BigInteger.Pow(2, (int)hbucket_id + 1) - 1;
            UInt128 max_range = UInt128.Parse(pow.ToString());
            
            if (distance <= max_range && distance >= min_range)
            {
                return hbucket_id;
            }
        }

        throw new Exception("Node does not fit into any h-bucket");
    }

    /// <summary>
    /// Calculate the node ID of given HNode parameters. Node ID is in big endian.
    /// </summary>
    /// <param name="ip_address"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static UInt128 CalculateNodeId(IPAddress ip_address, UInt16 port)
    {
        if (ip_address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            byte[] ip_bytes = ip_address.GetAddressBytes().ToArray();
            UInt64 upper = BitConverter.ToUInt64(ip_bytes.Skip(2).Take(8).ToArray());
            UInt64 lower =
                BitConverter.ToUInt64(ByteUtil.Concat(ip_bytes.Skip(10).Take(6).ToArray(),
                    BitConverter.GetBytes(port)));
            return new UInt128(upper, lower);
        }
        else if (ip_address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            ByteArrayBuilder builder = new ByteArrayBuilder();

            byte[] ip_address_bytes = ip_address.GetAddressBytes();
        
            if (!BitConverter.IsLittleEndian)
            {
                // Most significant bit first
                ip_address_bytes = ip_address_bytes.Reverse().ToArray();
            }

            // Take 4 bytes of the hash of the first three octets
            for (int i = 0; i < 3; i++)
            {
                byte[] octet_hash = Keccak.Hash256(new byte[1] { ip_address_bytes[i] }).Take(4).ToArray();
                builder.Append(octet_hash);
            }
        
            // Take 2 bytes of the hash of the last octet
            byte[] final_octet_hash = Keccak.Hash256(new byte[1] { ip_address_bytes[3] }).Take(2).ToArray();
            builder.Append(final_octet_hash);

            // Take 2 bytes of the port
            byte[] port_bytes = BitConverter.GetBytes(port).Reverse().ToArray();
            builder.Append(port_bytes);

            byte[] node_id_bytes = builder.Build();
            //Console.WriteLine("node_id_bytes: "+BitConverter.ToString(node_id_bytes));

            node_id_bytes = node_id_bytes.Reverse().ToArray();
            UInt64 upper = BitConverter.ToUInt64(node_id_bytes.Skip(8).Take(8).ToArray());
            UInt64 lower = BitConverter.ToUInt64(node_id_bytes.Take(8).ToArray());
            UInt128 node_id = new UInt128(upper, lower);
            //Console.WriteLine("node_id: "+node_id);

            return node_id;
        }

        throw new Exception("Invalid AddressFamily");
    }
}