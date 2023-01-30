// Copyright (C) 2023 ENX Foundation
//
// HyperNebula is free software distributed under the GNU Affero General Public License v3.0
// see the accompanying file LICENSE in the main directory of the project for more details.

using System.Net;

namespace HyperNebula.Network;

public class Peer : IPeer
{
	private readonly IPAddress ip_address;
	private readonly UInt16 port;

	public Peer(string ip_address, UInt16 port)
	{
		this.ip_address       = IPAddress.Parse(ip_address);
		this.port             = port;
	}

	public IPAddress GetIpAddress()
	{
		return ip_address;
	}

	public ushort GetPort()
	{
		return port;
	}

	public override string ToString()
	{
		return ip_address + ":" + port;
	}
}