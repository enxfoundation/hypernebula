// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using HyperNebula.Common;
using HyperNebula.Message;
using NUnit.Framework;

namespace HyperNebula.tests;

public class MessageParserTest
{
    [Test]
    public static void Test1()
    {
        byte[] response;
        List<HNode> peer_list;


        // Test with one IPV6 address
        response = new byte[21]
        {
            0x00, MessageType.RESPOND_PEERS, 0x33, 0x33,

            // IPV6 Address
            HyperNebulaProtocol.IPV6_FLAG, 
            0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF, // IPV6 address
            0x33, 0x33, // port
        };
        peer_list = MessageParser.ParseRespondPeers(response);
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[0].ip_address.GetAddressBytes(), new byte[16]{0x20, 0x01, 0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}));
        Assert.AreEqual(13107, peer_list[0].port);
        
        // Test with one IPV4 address
        response = new byte[11]
        {
            0x00, MessageType.RESPOND_PEERS, 0x33, 0x33,

            // IPV4 Address
            HyperNebulaProtocol.IPV4_FLAG,
            0xFF,0xFF,0xFF,0xFF, // IPV4 address
            0x33, 0x33, // port
        };
        peer_list = MessageParser.ParseRespondPeers(response);
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[0].ip_address.GetAddressBytes(), new byte[4]{0xFF, 0xFF, 0xFF, 0xFF}));
        Assert.AreEqual(13107, peer_list[0].port);

        // Test with one IPV4 address then one IPV6 address
        response = new byte[28]
        {
            0x00, MessageType.RESPOND_PEERS, 0x33, 0x33,

            // IPV4 Address
            HyperNebulaProtocol.IPV4_FLAG,
            0xFF,0xFF,0xFF,0xFF, // IPV4 address
            0x33, 0x33, // port
            
            // IPV6 Address
            HyperNebulaProtocol.IPV6_FLAG, 
            0x88,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF, // IPV6 address
            0x33, 0x33, // port
        };
        peer_list = MessageParser.ParseRespondPeers(response);
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[0].ip_address.GetAddressBytes(), new byte[4]{0xFF, 0xFF, 0xFF, 0xFF}));
        Assert.AreEqual(13107, peer_list[0].port);
        
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[1].ip_address.GetAddressBytes(), new byte[16]{0x20, 0x01, 0x88,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF}));
        Assert.AreEqual(13107, peer_list[1].port);
        
        // Test with one IPV4 address, one IPV6 address, then one IPV4 address
        response = new byte[35]
        {
            0x00, MessageType.RESPOND_PEERS, 0x33, 0x33,

            // IPV4 Address
            HyperNebulaProtocol.IPV4_FLAG,
            0xD0,0xFF,0xFF,0xD1, // IPV4 address
            0x33, 0x33, // port
            
            // IPV6 Address
            HyperNebulaProtocol.IPV6_FLAG, 
            0x88,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0x87, // IPV6 address
            0x33, 0x33, // port
            
            // IPV4 Address
            HyperNebulaProtocol.IPV4_FLAG,
            0xE0,0xFF,0xFF,0xE1, // IPV4 address
            0x33, 0x33, // port
        };
        peer_list = MessageParser.ParseRespondPeers(response);
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[0].ip_address.GetAddressBytes(), new byte[4]{0xD0, 0xFF, 0xFF, 0xD1}));
        Assert.AreEqual(13107, peer_list[0].port);
        
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[1].ip_address.GetAddressBytes(), new byte[16]{0x20, 0x01, 0x88,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0xFF,0x87}));
        Assert.AreEqual(13107, peer_list[1].port);
        
        Assert.AreEqual(true, ByteUtil.Equals(peer_list[2].ip_address.GetAddressBytes(), new byte[4]{0xE0, 0xFF, 0xFF, 0xE1}));
        Assert.AreEqual(13107, peer_list[2].port);
    }
}