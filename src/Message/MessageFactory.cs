// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using HyperNebula.Common;

namespace HyperNebula.Message;

public static class MessageFactory
{
    public static byte[] Ping(byte[] entropy)
    {
        ByteArrayBuilder builder = new ByteArrayBuilder();
        builder.Append(HyperNebulaProtocol.VERSION);
        builder.Append(MessageType.PING);
        builder.Append(HyperNebula.GetInstance().GetPort());
        builder.Append(entropy);
        return builder.Build();
    }

    public static byte[] Pong(byte[] entropy)
    {
        ByteArrayBuilder builder = new ByteArrayBuilder();
        builder.Append(HyperNebulaProtocol.VERSION);
        builder.Append(MessageType.PONG);
        builder.Append(HyperNebula.GetInstance().GetPort());
        builder.Append(entropy);
        return builder.Build();
    }
    
    public static byte[] RequestPeers()
    {
        ByteArrayBuilder builder = new ByteArrayBuilder();
        builder.Append(HyperNebulaProtocol.VERSION);
        builder.Append(MessageType.REQUEST_PEERS);
        builder.Append(HyperNebula.GetInstance().GetPort());
        return builder.Build();
    }
    
    public static byte[] RespondPeers(List<HNode> peer_list)
    {
        ByteArrayBuilder builder = new ByteArrayBuilder();
        builder.Append(HyperNebulaProtocol.VERSION);
        builder.Append(MessageType.RESPOND_PEERS);
        builder.Append(HyperNebula.GetInstance().GetPort());

        foreach (HNode node in peer_list)
        {
            if (node.ipv6)
            {
                builder.Append(HyperNebulaProtocol.IPV6_FLAG);
                builder.Append(node.ip_address.GetAddressBytes().Skip(2).Take(14).ToArray());
                builder.Append(BitConverter.GetBytes(node.port));
            }
            else
            {
                builder.Append(HyperNebulaProtocol.IPV4_FLAG);
                builder.Append(node.ip_address.GetAddressBytes());
                builder.Append(BitConverter.GetBytes(node.port));
            }
        }
        
        return builder.Build();
    }
}