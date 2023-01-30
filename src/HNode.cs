// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;

namespace HyperNebula;

public struct HNode
{
    public UInt128 id;
    public IPAddress ip_address;
    public UInt16 port;
    public bool ipv6;

    public HNode(IPAddress ip_address, UInt16 port)
    {
        this.ip_address = ip_address;
        this.port = port;
        
        UInt128 node_id = new UInt128(0, 0);
        if (ip_address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
        {
            // IPV6 Address
            node_id = HyperNebulaProtocol.CalculateNodeId(this.ip_address, port);
            this.ipv6 = true;
        }
        else if(ip_address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        {
            // IPV4 Address
            node_id = HyperNebulaProtocol.CalculateNodeId(this.ip_address, port);
            this.ipv6 = false;
        }
        else
        {
            throw new Exception("Invalid IPAddress family");
        }
        
        this.id = node_id;
    }

    public UInt128 GetId()
    {
        return this.id;
    }
}