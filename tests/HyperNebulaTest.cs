// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using System.Security.Cryptography;
using HyperNebula.Common;
using HyperNebula.Message;
using NUnit.Framework;

namespace HyperNebula.tests;

public class HyperNebulaTest
{
    /// <summary>
    /// Generate IP addresses randomly and attempt to import. Then, verify import was successful.
    /// </summary>
    [Test]
    public static void Test1()
    {
        HyperNebula.Initialize(IPAddress.Parse("127.0.0.1"), 13500);

        RNGCryptoServiceProvider rng = new();

        // Import 10k IPV4 addresses
        for (int i = 0; i < 10_000; i++)
        {
            HNode hnode =
                new HNode(
                    IPAddress.Parse(RandomUInt8(rng) + "." + RandomUInt8(rng) + "." + RandomUInt8(rng) + "." +
                                    RandomUInt8(rng)), (UInt16)(1 + (RandomUInt8(rng) * 128)));
            HyperNebula.GetInstance().ImportNode(hnode);
        }
        
        // Import 10k IPV6 addresses
        for (int i = 0; i < 10_000; i++)
        {
            byte[] bytes = new byte[14];
            rng.GetBytes(bytes);
            bytes = ByteUtil.Concat(new byte[2] { 0x20, 0x01 }, bytes); // global unicast
            
            HNode hnode =
                new HNode(
                    new IPAddress(bytes), (UInt16)(1 + (RandomUInt8(rng) * 128)));
            HyperNebula.GetInstance().ImportNode(hnode);
        }

        List<HNode> peer_list = HyperNebula.GetInstance().GetAllKnownPeers();
        
        byte[] peer_list_bytes = MessageFactory.RespondPeers(peer_list);

        List<HNode> peer_list_response = MessageParser.ParseRespondPeers(peer_list_bytes);

        byte[] peer_list_verify_bytes = MessageFactory.RespondPeers(peer_list_response);

        Assert.AreEqual(true, ByteUtil.Equals(peer_list_bytes, peer_list_verify_bytes));
    }
    
    private static UInt16 RandomUInt8(RNGCryptoServiceProvider rng)
    {
        byte[] bytes = new byte[1];
        rng.GetBytes(bytes);
        UInt16 octet = BitConverter.ToUInt16(ByteUtil.Concat(bytes, new byte[1]{0x00}));
        return octet;
    }
}