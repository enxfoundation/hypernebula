// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using HyperNebula.Common;
using NUnit.Framework;

namespace HyperNebula.tests;

[TestFixture]
public class HyperNebulaProtocolTest
{
    [SetUp]
    public void SetUp()
    {
    }

    /// <summary>
    /// In this test, distance is expected to incrementally increase, as
    /// the IP addresses are further away from each other.
    /// </summary>
    [Test]
    private static void Test1()
    {
        UInt128 prev = Test1Iterator("127.0.0.1", 5000, "127.0.0.1", 5001);
        
        UInt128 distance = Test1Iterator("127.0.0.1", 5000, "127.0.0.2", 5000);
        Assert.That(distance, Is.GreaterThan(prev));
        
        distance = Test1Iterator("127.0.0.1", 5000, "127.0.1.1", 5000);
        Assert.That(distance, Is.GreaterThan(prev));
        
        distance = Test1Iterator("127.0.0.1", 5000, "127.1.0.1", 5000);
        Assert.That(distance, Is.GreaterThan(prev));
        
        distance = Test1Iterator("127.0.0.1", 5000, "128.0.0.1", 5000);
        Assert.That(distance, Is.GreaterThan(prev));
    }

    /// <summary>
    /// Generates a node id given a byte array
    /// </summary>
    /// <param name="node_id_bytes"></param>
    /// <returns>node ID</returns>
    private static UInt128 Test2Iterator(byte[] node_id_bytes)
    {
        node_id_bytes = node_id_bytes.Reverse().ToArray();
        UInt64 upper = BitConverter.ToUInt64(node_id_bytes.Skip(8).Take(8).ToArray());
        UInt64 lower = BitConverter.ToUInt64(node_id_bytes.Take(8).ToArray());
        return new UInt128(upper, lower);
    }
    
    /// <summary>
    /// Generate byte arrays from 0xFF...00 to 0x00...FF
    /// and ensure there are no duplicates
    /// </summary>
    [Test]
    public static void Test2()
    {
        HashSet<UInt128> hashset = new();
        for (int i = 0; i < 16; i++)
        {
            byte[] node_id_bytes = Array.Empty<byte>();
            for (int j = 0; j < 16; j++)
            {
                if (i == j)
                {
                    node_id_bytes = ByteUtil.AppendBytes(node_id_bytes, 0xff);
                }
                else
                {
                    node_id_bytes = ByteUtil.AppendBytes(node_id_bytes, 0x00);
                }
            }

            UInt128 node_id = Test2Iterator(node_id_bytes);
            Assert.AreEqual(false , hashset.Contains(node_id));
            hashset.Add(node_id);
        }
    }
    
    /// <summary>
    /// Generates two h-nodes, then returns the distance between them.
    /// </summary>
    /// <returns></returns>
    private static UInt128 Test1Iterator(string ip_a, UInt16 port_a, string ip_b, UInt16 port_b)
    {
        HNode a = new HNode(IPAddress.Parse(ip_a), port_a);
        HNode b = new HNode(IPAddress.Parse(ip_b), port_b);
        return HyperNebulaProtocol.CalculateDistance(a.id, b.id);
    }
}