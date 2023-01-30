// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;
using HyperNebula.Common;

namespace HyperNebula.Message;

public static class MessageParser
{
    private const int IPV6_FLAG_BYTES = 1;
    private const int IPV4_BYTES = 4;
    private const int IPV6_USABLE_BYTES = 14;
    private const int PORT_BYTES = 2;

    public static MessageType DetectType(byte[] raw_message)
    {
        byte version = raw_message[0];
        if (version != HyperNebulaProtocol.VERSION)
        {
            throw new Exception("Protocol version mismatch");
        }

        switch (raw_message[1])
        {
            case MessageType.PING:
            case MessageType.PONG:
            case MessageType.REQUEST_PEERS:
            case MessageType.RESPOND_PEERS:
                return new MessageType(raw_message[1]);
                break;
        }

        throw new Exception("Invalid MessageType encountered");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="raw_message"></param>
    /// <returns>Ping entropy</returns>
    public static new Tuple<byte[], UInt16> ParsePing(byte[] raw_message)
    {
        UInt16 remote_port = BitConverter.ToUInt16(raw_message.Skip(2).Take(2).ToArray());
        byte[] entropy = ParsePong(raw_message);
        return new Tuple<byte[], ushort>(entropy, remote_port);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="raw_message"></param>
    /// <returns>Pong entropy</returns>
    public static byte[] ParsePong(byte[] raw_message)
    {
        return raw_message.Skip(4).Take(raw_message.Length - 2).ToArray();
    }

    public static List<HNode> ParseRespondPeers(byte[] response)
    {
        Dictionary<UInt128, HNode> new_peers = new Dictionary<UInt128, HNode>();

        // Remote port of the requesting h-node
        // UInt16 remote_port = BitConverter.ToUInt16(response.Skip(2).Take(2).ToArray());

        // remove VERSION, MessageType, and Port
        response = response.Skip(4).Take(response.Length - 1).ToArray();

        for (int offset = 0; offset < response.Length; offset++)
        {
            // 1 byte for ipv6 flag, 4 bytes for minimum address bytes (ipv4), and 2 bytes for port = 7 total bytes expected
            if (offset + 7 > response.Length) // check if sufficient data left
            {
                return new List<HNode>(PeerDictionaryToList(new_peers));
            }

            byte ipv6_flag = response[offset];

            byte[] ip_address_bytes;
            UInt16 port;
            if (ipv6_flag == HyperNebulaProtocol.IPV6_FLAG)
            {
                // {0x20, 0x01} is ipv6 global unicast
                ip_address_bytes =
                    ByteUtil.Concat(new byte[2] { 0x20, 0x01 }, response.Skip(offset+IPV6_FLAG_BYTES).Take(IPV6_USABLE_BYTES).ToArray());
                port = BitConverter.ToUInt16(response.Skip(offset + IPV6_FLAG_BYTES + IPV6_USABLE_BYTES).Take(PORT_BYTES).ToArray());
            }
            else if(ipv6_flag == HyperNebulaProtocol.IPV4_FLAG)
            {
                ip_address_bytes = response.Skip(offset+IPV6_FLAG_BYTES).Take(IPV4_BYTES).ToArray();
                port = BitConverter.ToUInt16(response.Skip(offset + IPV6_FLAG_BYTES + IPV4_BYTES).Take(PORT_BYTES).ToArray());
            }
            else
            {
                Log.Warn("HyperNebula - Malformed data encountered", "e=invalid ipv6_flag '"+ipv6_flag+"'");
                return new List<HNode>(PeerDictionaryToList(new_peers));
            }
            IPAddress ip_address = new IPAddress(ip_address_bytes);
            
            HNode hnode = new HNode(ip_address, port);
            if (!new_peers.ContainsKey(hnode.GetId()))
            {
                new_peers[hnode.id] = hnode;
            }

            // Increment offset
            if (ipv6_flag == HyperNebulaProtocol.IPV6_FLAG)
            {
                offset += IPV6_FLAG_BYTES + IPV6_USABLE_BYTES + 2 -1; // 2 = port bytes
            }
            else
            {
                offset += IPV6_FLAG_BYTES + IPV4_BYTES + 2 -1;
            }
        }

        return new List<HNode>(PeerDictionaryToList(new_peers));
    }
    
    private static List<HNode> PeerDictionaryToList(Dictionary<UInt128, HNode> dictionary)
    {
        List<HNode> peer_list = new List<HNode>();
        foreach (KeyValuePair<UInt128, HNode> kv in dictionary)
        {
            peer_list.Add(kv.Value);
        }

        return peer_list;
    }
}